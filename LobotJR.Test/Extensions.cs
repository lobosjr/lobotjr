namespace LobotJR.Test
{
    public static class Extensions
    {
        public static bool DeeplyEquals(this object item1, object item2)
        {
            var type = item1.GetType();
            var type2 = item2.GetType();
            if (type != type2)
            {
                return false;
            }
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var value1 = property.GetValue(item1);
                var value2 = property.GetValue(item2);
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                {
                    if (!value1.Equals(value2))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!value1.DeeplyEquals(value2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
