using System;
using System.Collections.Generic;
using System.Linq;
using static Expr;
using static Function;

public interface IExpr
{
    IEnumerable<string> Variables { get; }
    bool IsConstant { get; }
    bool IsPolynomial { get; }
    int PolynomialDegree { get; }
    double Compute(IReadOnlyDictionary<string, double> variableValues);
}

public abstract class Expr : IExpr
{   
    public abstract IEnumerable<string> Variables { get; }
    public abstract bool IsConstant { get; }
    public abstract bool IsPolynomial { get; }
    public abstract int PolynomialDegree { get; }
    public abstract double Compute(IReadOnlyDictionary<string, double> variableValues = null);

    public static implicit operator Expr(int arg) => new Constant(arg);
    public static Expr Sqrt(Expr operand) => new Sqrt(operand);


    //Unary
    
    public static Expr operator +(Expr operand) => new UnaryPlus(operand);
    public static Expr operator -(Expr operand) => new UnaryMinus(operand);
    //Binary
    public static Expr operator +(Expr a, Expr b) => new BinaryAddition(a, b);
    public static Expr operator -(Expr a, Expr b) => new BinarySubtraction(a, b);
    public static Expr operator *(Expr a, Expr b) => new Multiplication(a, b);
    public static Expr operator /(Expr a, Expr b) => new Division(a, b);
}

public class Constant : Expr
{
    public double Value { get; }
    
    public Constant(double value) => Value = value;

    public override IEnumerable<string> Variables => Enumerable.Empty<string>();
    public override bool IsConstant => true;
    public override bool IsPolynomial => true;
    public override int PolynomialDegree => 0;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Value;
    public override string ToString() => Value.ToString();
}

public class Variable : Expr
{
    public string Name { get; }

    public Variable(string name) => Name = name;

    public override IEnumerable<string> Variables => new[] { Name };
    public override bool IsConstant => false;
    public override bool IsPolynomial => true;
    public override int PolynomialDegree => 1;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
        variableValues.ContainsKey(Name) ? variableValues[Name] : throw new ArgumentException($"Variable {Name} is not defined.");
    public override string ToString() => Name;
}

public abstract class UnaryOperation : Expr
{
    public Expr Operand { get; }

    public UnaryOperation(Expr operand) => Operand = operand;

    public override IEnumerable<string> Variables => Operand.Variables;
    public override bool IsConstant => Operand.IsConstant;
    public override bool IsPolynomial => false;
    public override int PolynomialDegree => 0;
}

public class UnaryPlus : UnaryOperation
{
    public UnaryPlus(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues = null) => Operand.Compute(variableValues);
    public override string ToString() => $"+({Operand})";
}

public class UnaryMinus : UnaryOperation
{
    public UnaryMinus(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues = null) => -Operand.Compute(variableValues);
    public override string ToString() => $"-({Operand})";
}

public abstract class BinaryOperation : Expr
{
    public Expr A { get; }
    public Expr B { get; }

    public BinaryOperation(Expr a, Expr b)
    {
        A = a;
        B = b;
    }

    public override IEnumerable<string> Variables => A.Variables.Union(B.Variables);
    public override bool IsConstant => A.IsConstant && B.IsConstant;
    public override bool IsPolynomial => A.IsPolynomial && B.IsPolynomial;
    public override int PolynomialDegree => Math.Max(A.PolynomialDegree, B.PolynomialDegree);
}

public class BinaryAddition : BinaryOperation
{
    public BinaryAddition(Expr left, Expr right) : base(left, right) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) + B.Compute(variableValues);
    public override string ToString() => $"({A} + {B})";
}

public class BinarySubtraction : BinaryOperation
{
    public BinarySubtraction(Expr a, Expr b) : base(a, b) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) - B.Compute(variableValues);
    public override string ToString() => $"({A} - {B})";
}

public class Multiplication : BinaryOperation
{
    public Multiplication(Expr a, Expr b) : base(a, b) { }

    public override int PolynomialDegree => A.PolynomialDegree + B.PolynomialDegree;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) * B.Compute(variableValues);
    public override string ToString() => $"({A} * {B})";
}

public class Division : BinaryOperation
{
    public Division(Expr a, Expr b) : base(a, b) { }

    public override bool IsPolynomial => A.IsPolynomial && B.IsPolynomial && B.PolynomialDegree == 0;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues)
    {
        double divisor = B.Compute(variableValues);
        if (divisor == 0)
            throw new DivideByZeroException("Division by zero is not allowed.");
        return A.Compute(variableValues) / divisor;
    }
    public override string ToString() => $"({A} / {B})";
}

public class Sqrt : UnaryOperation
{
    public Sqrt(Expr operand) : base(operand) { }

    public override IEnumerable<string> Variables => Operand.Variables;
    public override bool IsConstant => Operand.IsConstant;
    public override bool IsPolynomial => false;
    public override int PolynomialDegree => 0;

    public override double Compute(IReadOnlyDictionary<string, double> variableValues)
    {
        double operandValue = Operand.Compute(variableValues);
        if (operandValue < 0)
            throw new ArgumentException("Square root of a negative number is not allowed.");
        return Math.Sqrt(operandValue);
    }

    public override string ToString() => $"Sqrt({Operand})";
}
public abstract class Function:Expr
{
    public Expr Val{ get; }
    public Function(Expr val) => Val = val;
    public override IEnumerable<string> Variables => Val.Variables;
    public override bool IsConstant => Val.IsConstant;
    public override bool IsPolynomial => Val.IsPolynomial;
    public override int PolynomialDegree => Val.PolynomialDegree;
    public static Expr Sin(Expr operand) => new Sin(operand);
    public static Expr Cos(Expr operand) => new Cos(operand);
    public static Expr Tan(Expr operand) => new Tan(operand);
    public static Expr Ctg(Expr operand) => new Ctg(operand);

}
public class Sin : Function
{
    public Sin(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
        Math.Sin((Val.Compute(variableValues)));

    public override string ToString() => $"Sin({Val})";
}

public class Cos : Function
{
    public Cos(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
        Math.Cos((Val.Compute(variableValues)));

    public override string ToString() => $"Cos({Val})";
}

public class Tan : Function
{
    public Tan(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
        Math.Tan((Val.Compute(variableValues)));

    public override string ToString() => $"Tan({Val})";
}
public class Ctg : Function
{
    public Ctg(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
        1 / Math.Tan((Val.Compute(variableValues)));

    public override string ToString() => $"Ctg({Val})";
}

class Program{
    
    static void Main(string[] args){
        //Тестирование
        var x = new Variable("x");
        var y = new Variable("y");
        var c = new Constant(3);
        var expr1 = (x - 4) * (3*x + y*y) / 5;
        var expr2 = (5 - 3*c) * Sqrt(16 + c*c);
       Console.WriteLine($"""
        {expr1.ToString()}
        [{string.Join(", ", expr1.Variables)}]
        {expr1.IsConstant}
        {expr1.IsPolynomial}
        {expr1.PolynomialDegree}
        {expr1.Compute(new Dictionary<string, double> { { "x", 1 }, { "y", 2 } })}
        """);
       Console.WriteLine($"""
        {expr2.ToString()}
        [{string.Join(", ", expr2.Variables)}]
        {expr2.IsConstant}
        {expr2.IsPolynomial}
        {expr2.PolynomialDegree}
        {expr2.Compute(new Dictionary<string, double> { { "x", 1 }, { "y", 2 } })}
        """);
        var z = new Variable("z");
        var expr3 =  Ctg(z);
        Console.WriteLine($"{expr3.ToString()}");  
        Console.WriteLine($" {expr3.Compute(new Dictionary<string, double> { { "z", 0.523599 }})}");
        
    }

}