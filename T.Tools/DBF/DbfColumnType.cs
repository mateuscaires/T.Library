namespace T.DBF
{
    public enum DBFColumnType
    {
        Character = 'C',
        Number = 'N',
        Logical = 'L',
        Date = 'D',
        Memo = 'M',
        FloatingPoint = 'F',
        //Character name variable 
        Binary = 'B',
        General = 'G',
        Picture = 'P',
        Currency = 'Y',
        DateTime = 'T',
        Integer = 'I',
        VariField = 'V',
        //Variant (X) for compatibility with SQL-s (i.e. varChar). 
        Timestamp = '@',
        Double = 'O',
        Autoincrement = '+',
    }
}
