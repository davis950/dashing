﻿namespace Dashing.Migration {
    using System.Collections.Generic;

    using Dashing.Configuration;

    public interface IMigrator {
        string GenerateSqlDiff(
            IEnumerable<IMap> fromMaps,
            IEnumerable<IMap> toMaps,
            IAnswerProvider answerProvider,
            IEnumerable<string> indexesToIgnore,
            IEnumerable<string> tablesToIgnore,
            out IEnumerable<string> warnings,
            out IEnumerable<string> errors);
    }
}