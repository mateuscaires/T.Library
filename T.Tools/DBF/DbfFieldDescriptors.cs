namespace T.DBF
{
    public static class DBFFieldDescriptors
    {
        private const byte doubleLength = 19;

        private const byte doubleDecimalCount = 11;

        private const byte integerLength = 9;

        private const byte dateLength = 8;

        private const byte booleanLength = 1;

        private const byte maxStringLength = 255;

        public static DBFFieldDescriptor GetDoubleField(string fieldName)
        {
            return new DBFFieldDescriptor(fieldName, (char)DBFColumnType.FloatingPoint, doubleLength, doubleDecimalCount);
        }

        public static DBFFieldDescriptor GetIntegerField(string fieldName)
        {
            return new DBFFieldDescriptor(fieldName, (char)DBFColumnType.Number, integerLength, 0);
        }

        public static DBFFieldDescriptor GetStringField(string fieldName)
        {
            return GetStringField(fieldName, maxStringLength);
        }

        public static DBFFieldDescriptor GetStringField(string fieldName, byte length)
        {
            return new DBFFieldDescriptor(fieldName, (char)DBFColumnType.Character, length, 0);
        }

        public static DBFFieldDescriptor GetBooleanField(string fieldName)
        {
            return new DBFFieldDescriptor(fieldName, (char)DBFColumnType.Logical, booleanLength, 0);
        }
    }
}
