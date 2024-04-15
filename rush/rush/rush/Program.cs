using rush;
using rush.traitImpl1;

unsafe void FuncRc(Rc<Person> rc)
{
    rc.Obj->Age++;
    Console.WriteLine($"Current Value: {rc.Obj->Age}");
}

PinnedRc<Person> pin;

unsafe void TestRC()
{
    Console.WriteLine();
    Console.WriteLine("[TestRC]");
    using var rc = new Rc<Person>();
    rc.Obj->Age = 5;
    FuncRc(rc);
    pin = rc.Pin();
    Console.WriteLine(pin.Obj->Age);
}

unsafe void TestAuto()
{
    Console.WriteLine();
    Console.WriteLine("[TestAuto]");
    using var person = new Auto<Person>();
    person.Obj->Age = 5;
    person.Obj->ShowAge();
}

TestAuto();
TestRC();

Console.WriteLine();
pin.Dispose();
