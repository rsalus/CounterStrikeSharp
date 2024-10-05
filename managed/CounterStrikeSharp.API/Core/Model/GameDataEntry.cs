namespace CounterStrikeSharp.API.Core.Model
{
    public class GameDataEntry
    {
        public Signatures? Signatures { get; internal set; }
        public Offsets? Offsets { get; internal set; }

        // Private constructor to enforce builder usage
        private GameDataEntry() { }

        public class Builder
        {
            private readonly GameDataEntry _entry = new();

            public Builder WithSignatures(Signatures signatures)
            {
                _entry.Signatures = signatures;
                return this;
            }

            public Builder WithOffsets(Offsets offsets)
            {
                _entry.Offsets = offsets;
                return this;
            }

            public GameDataEntry Build()
            {
                return _entry;
            }
        }
    }
}
