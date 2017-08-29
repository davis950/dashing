﻿namespace Dashing.CodeGeneration.Weaving.Weavers {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class SetLoggerWeaver : BaseWeaver {
        public override void Weave(
            AssemblyDefinition assemblyDefinition,
            TypeDefinition typeDefinition,
            IEnumerable<ColumnDefinition> columnDefinitions) {
            // this gets called with a typeDef set to something that's being mapped
            // but there's the possibility of each column belonging to a different parent class
            // so we'll find all of the class hierarchy and weave them individually
            var classHierarchy = this.GetClassHierarchy(typeDefinition);
            var totalInChain = classHierarchy.Count;

            // if there's only one class and that class is not extended elsewhere we'll use non-virtual methods
            var notInInheritance = totalInChain == 1
                                   && !assemblyDefinition.MainModule.Types.Any(
                                       t => t.IsClass && t.BaseType != null && t.BaseType.FullName == typeDefinition.FullName);
            while (classHierarchy.Count > 0) {
                this.ImplementISetLoggerForTypeDefinition(classHierarchy.Pop(), columnDefinitions, notInInheritance);
            }
        }

        private void ImplementISetLoggerForTypeDefinition(
            TypeDefinition typeDef,
            IEnumerable<ColumnDefinition> columnDefinitions,
            bool notInInheritance) {
            if (typeDef.Methods.Any(m => m.Name == "GetSetProperties")) {
                return; // this type has already been woven, exit
            }

            if (!typeDef.ImplementsInterface(typeof(ISetLogger))) {
                this.AddInterfaceToNonObjectAncestor(typeDef, typeof(ISetLogger));
            }

            // some common type definitions
            var boolTypeDef = typeDef.Module.ImportReference(typeof(bool));
            var stringTypeDef = typeDef.Module.ImportReference(typeof(string));
            var listStringTypeDef = typeDef.Module.ImportReference(typeof(List<>)).MakeGenericInstanceType(stringTypeDef);
            var nonPkCols = columnDefinitions.Where(c => !c.IsPrimaryKey && c.Relationship != RelationshipType.OneToMany).ToList();

            // add the fields for tracking which properties are set
            // and insert the code in to the setter
            foreach (var columnDefinition in nonPkCols) {
                if (this.HasPropertyInInheritanceChain(typeDef, columnDefinition.Name)) {
                    var propDef = this.GetProperty(typeDef, columnDefinition.Name);
                    if (propDef.DeclaringType.FullName == typeDef.FullName) {
                        // columns in parent classes will have been taken care of
                        var isSetFieldDef = new FieldDefinition(
                            string.Format("__{0}_IsSet", columnDefinition.Name),
                            FieldAttributes.Family,
                            boolTypeDef);
                        this.MakeNotDebuggerBrowsable(typeDef.Module, isSetFieldDef);
                        propDef.DeclaringType.Fields.Add(isSetFieldDef);

                        // assign true to this field
                        var il = propDef.SetMethod.Body.Instructions;
                        il.Insert(0, Instruction.Create(OpCodes.Stfld, isSetFieldDef));
                        il.Insert(0, Instruction.Create(OpCodes.Ldc_I4_1));
                        il.Insert(0, Instruction.Create(OpCodes.Ldarg_0));
                    }
                }
            }

            // implement the interface
            var methodAttrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (notInInheritance) {
                methodAttrs = methodAttrs | MethodAttributes.NewSlot | MethodAttributes.Final;
            }

            var getSetProperties = new MethodDefinition(
                "GetSetProperties",
                methodAttrs,
                typeDef.Module.ImportReference(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef));
            getSetProperties.Body.Variables.Add(new VariableDefinition(listStringTypeDef));
            getSetProperties.Body.Variables.Add(
                new VariableDefinition(
                    typeDef.Module.ImportReference(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef)));
            getSetProperties.Body.Variables.Add(new VariableDefinition(boolTypeDef));
            getSetProperties.Body.InitLocals = true;
            var instructions = getSetProperties.Body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            var listStringContruictor = MakeGeneric(
                typeDef.Module.ImportReference(
                    listStringTypeDef.Resolve().GetConstructors().First(c => !c.HasParameters && !c.IsStatic && c.IsPublic)),
                stringTypeDef);
            instructions.Add(Instruction.Create(OpCodes.Newobj, listStringContruictor));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));

            var breakToInstruction = Instruction.Create(nonPkCols.Count == 1 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            var addMethod = typeDef.Module.ImportReference(listStringTypeDef.Resolve().Methods.Single(m => m.Name == "Add"));
            addMethod = MakeGeneric(addMethod, stringTypeDef);

            var visibleCols = nonPkCols.Where(c => this.HasPropertyInInheritanceChain(typeDef, c.Name)).ToList();
            for (var i = 0; i < visibleCols.Count; i++) {
                if (i == 0) {
                    instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                }

                instructions.Add(
                    Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, string.Format("__{0}_IsSet", visibleCols.ElementAt(i).Name))));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Stloc_2));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_2));
                instructions.Add(Instruction.Create(OpCodes.Brtrue, breakToInstruction));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, visibleCols.ElementAt(i).Name));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, addMethod));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(breakToInstruction);
                breakToInstruction = Instruction.Create(i == visibleCols.Count - 2 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            }

            instructions.Add(Instruction.Create(OpCodes.Stloc_1));
            var retInstr = Instruction.Create(OpCodes.Ldloc_1);
            instructions.Add(Instruction.Create(OpCodes.Br, retInstr));
            instructions.Add(retInstr);
            instructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getSetProperties);
        }
    }
}