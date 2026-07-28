using System;
using System.Collections.Generic;

namespace TapMinies.Core
{
    [Serializable]
    public class SaveData
    {
        public long gold;
        public int currentStage = 1;
        public int highestStageCleared;
        public List<int> heroLevels = new List<int>();
    }
}
