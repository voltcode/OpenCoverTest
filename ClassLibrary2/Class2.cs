using ClassLibrary1;

namespace ClassLibrary2
{
    public class Class2
    {
        public static int M2()
        {
            //Class1.M1();
            //Class1.M1();
            //Class1.M1();

            return Class1.M1() + 2;
        }
    }
}
