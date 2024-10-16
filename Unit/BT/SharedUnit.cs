using BehaviorDesigner.Runtime;

[System.Serializable]
    public class SharedUnit : SharedVariable<Unit>
    {
        public static implicit operator SharedUnit(Unit value) { return new SharedUnit { Value = value }; }
    }