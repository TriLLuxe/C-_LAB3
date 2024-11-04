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
    public abstract double Compute(IReadOnlyDictionary<string, double> variableValues);

    public static implicit operator Expr(double arg) => new Constant(arg);
    public static Expr Sqrt(Expr operand) => new Sqrt(operand);

    // Унарные операторы
    public static Expr operator +(Expr operand) => new UnaryPlus(operand);
    public static Expr operator -(Expr operand) => new UnaryMinus(operand);
    // Бинарные операторы
    public static Expr operator +(Expr a, Expr b) => Simplifier.Simplify(new BinaryAddition(a, b));
    public static Expr operator -(Expr a, Expr b) => Simplifier.Simplify(new BinarySubtraction(a, b));
    public static Expr operator *(Expr a, Expr b) => Simplifier.Simplify(new Multiplication(a, b));
    public static Expr operator /(Expr a, Expr b) => Simplifier.Simplify(new Division(a, b));
}

public static class Simplifier
{
    public static Expr Simplify(Expr expr)
    {
        if (expr is BinaryAddition addition)
        {
            // x + 0 = x
            if (addition.B is Constant b && b.Value == 0)
            {
                return addition.A;
            }
            // 0 + x = x
            if (addition.A is Constant a && a.Value == 0)
            {
                return addition.B;
            }
            // Объединение одинаковых слагаемых
            if (addition.A.Equals(addition.B))
            {
                return new Multiplication(new Constant(2), addition.A);
            }
            // Случай суммирования нескольких одинаковых переменных
            if (addition.A is Multiplication multA && multA.B is Constant aVal && addition.B.Equals(multA.A))
            {
                return new Multiplication(new Constant(aVal.Value + 1), multA.A);
            }
            if (addition.B is Multiplication multB && multB.B is Constant bVal && addition.A.Equals(multB.A))
            {
                return new Multiplication(new Constant(bVal.Value + 1), multB.A);
            }
        }
        else if (expr is BinarySubtraction subtraction)
        {
            // x - 0 = x
            if (subtraction.B is Constant b && b.Value == 0)
            {
                return subtraction.A;
            }
            // x - x = 0
            if (subtraction.A.Equals(subtraction.B))
            {
                return new Constant(0);
            }
        }
        else if (expr is Multiplication multiplication)
        {
            // x * 0 = 0 или 0 * x = 0
            if (multiplication.A is Constant a && a.Value == 0 || multiplication.B is Constant b && b.Value == 0)
            {
                return new Constant(0);
            }
            // x * 1 = x
            if (multiplication.A is Constant a1 && a1.Value == 1)
            {
                return multiplication.B;
            }
            if (multiplication.B is Constant b1 && b1.Value == 1)
            {
                return multiplication.A;
            }
            // x * x = x^2
            if (multiplication.A.Equals(multiplication.B))
            {
                return new Power(multiplication.A, new Constant(2));
            }
            // Объединение степеней при умножении
            if (multiplication.A is Power powerA && multiplication.B is Power powerB && powerA.Base.Equals(powerB.Base))
            {
                return new Power(powerA.Base, new BinaryAddition(powerA.Exponent, powerB.Exponent));
            }
            if (multiplication.A is Power powerA2 && multiplication.B.Equals(powerA2.Base))
            {
                return new Power(powerA2.Base, new BinaryAddition(powerA2.Exponent, new Constant(1)));
            }
            if (multiplication.B is Power powerB2 && multiplication.A.Equals(powerB2.Base))
            {
                return new Power(powerB2.Base, new BinaryAddition(powerB2.Exponent, new Constant(1)));
            }
        }
        else if (expr is Division division)
        {
            // x / x = 1 (предполагается, что x != 0)
            if (division.A.Equals(division.B))
            {
                return new Constant(1);
            }
            // x / 1 = x
            if (division.B is Constant b2 && b2.Value == 1)
            {
                return division.A;
            }
            // 0 / x = 0 (предполагается, что x != 0)
            if (division.A is Constant a && a.Value == 0)
            {
                return new Constant(0);
            }
            // Упрощение деления степеней с одинаковыми основаниями
            if (division.A is Power powerA && division.B is Power powerB && powerA.Base.Equals(powerB.Base))
            {
                return new Power(powerA.Base, new BinarySubtraction(powerA.Exponent, powerB.Exponent));
            }
            if (division.A is Power powerA2 && division.B.Equals(powerA2.Base))
            {
                return new Power(powerA2.Base, new BinarySubtraction(powerA2.Exponent, new Constant(1)));
            }
        }

        return expr;
    }
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

    public override bool Equals(object obj) => obj is Constant constant && Value == constant.Value;
    public override int GetHashCode() => Value.GetHashCode();
}

public class Variable : Expr
{
    public string Name { get; }

    public Variable(string name) => Name = name;

    public override IEnumerable<string> Variables
    {
        get { yield return Name; }
    }
    public override bool IsConstant => false;
    public override bool IsPolynomial => true;
    public override int PolynomialDegree => 1;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues)
    {
        if (variableValues.TryGetValue(Name, out double value)) return value;
        else throw new ArgumentException($"Переменная {Name} не определена.");
    }

    public override string ToString() => Name;

    public override bool Equals(object obj) => obj is Variable variable && Name == variable.Name;
    public override int GetHashCode() => Name.GetHashCode();
}

public class Power : Expr
{
    public Expr Base { get; }
    public Expr Exponent { get; }

    public Power(Expr @base, Expr exponent)
    {
        Base = @base;
        Exponent = exponent;
    }

    public override IEnumerable<string> Variables => Base.Variables;
    public override bool IsConstant => Base.IsConstant && Exponent.IsConstant;
    public override bool IsPolynomial => Exponent is Constant exp && exp.Value == (int)exp.Value && exp.Value >= 0;
    public override int PolynomialDegree => Exponent is Constant exp ? (int)(Base.PolynomialDegree * exp.Value) : 0;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Pow(Base.Compute(variableValues), Exponent.Compute(variableValues));
    public override string ToString() => $"{Base}^{Exponent}";

    public override bool Equals(object obj) => obj is Power power && Base.Equals(power.Base) && Exponent.Equals(power.Exponent);
    public override int GetHashCode() => HashCode.Combine(Base, Exponent);
}
public abstract class UnaryOperation : Expr
{
    public Expr Operand { get; }

    public UnaryOperation(Expr operand) => Operand = operand;

    public override IEnumerable<string> Variables => Operand.Variables;
    public override bool IsConstant => Operand.IsConstant;
    public override bool IsPolynomial => Operand.IsPolynomial;
    public override int PolynomialDegree => Operand.PolynomialDegree;
}
public class UnaryPlus : UnaryOperation
{
    public UnaryPlus(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Operand.Compute(variableValues);
    public override string ToString() => $"+({Operand})";
}

public class UnaryMinus : UnaryOperation
{
    public UnaryMinus(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => -Operand.Compute(variableValues);
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
    
    public override bool IsConstant => A.IsConstant && B.IsConstant;//тут неверно (тут больше случаев)
    public override bool IsPolynomial => A.IsPolynomial && B.IsPolynomial;//тут неверно 
    public override int PolynomialDegree => Math.Max(A.PolynomialDegree, B.PolynomialDegree);//тут неверно 
}

public class BinaryAddition : BinaryOperation
{
    public BinaryAddition(Expr left, Expr right) : base(left, right) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) + B.Compute(variableValues);
    public override string ToString() => $"({A} + {B})";
}

public class BinarySubtraction : BinaryOperation //например тест x^2-x^2 выведет что степень полинома 2. а должна 0
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

public class Division : BinaryOperation// в делении не переопределеана степень (PolynomialDegree), тк максимум из 2 но для деления это не так
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

public class Sqrt : Function
{
    public Sqrt(Expr operand) : base(operand) { }

    public override IEnumerable<string> Variables => Val.Variables;
    
    public override double Compute(IReadOnlyDictionary<string, double> variableValues)
    {
        double operandValue = Val.Compute(variableValues);
        if (operandValue < 0)
            throw new ArgumentException("Square root of a negative number is not allowed.");
        return Math.Sqrt(operandValue);
    }

    public override string ToString() => $"Sqrt({Val})";
}
public abstract class Function:Expr
{
    public Expr Val{ get; }
    public Function(Expr val) => Val = val;
    public override IEnumerable<string> Variables => Val.Variables;
    // public override bool IsConstant => this is Sqrt ? Val.IsConstant : true;
    // public override int PolynomialDegree => this is Sqrt ? Val.PolynomialDegree /2 : 0; 
    // public override bool IsPolynomial => this is Sqrt ? Val.PolynomialDegree /2 >0? true: false: false;
    // public override bool IsConstant => true;
    // public override int PolynomialDegree =>0;
    // public override bool IsPolynomial => false;
    public override bool IsConstant => Val.IsConstant;
    public override int PolynomialDegree => this is Sqrt && Val.IsPolynomial && Val.PolynomialDegree % 2 == 0 
    ? Val.PolynomialDegree / 2 : 0;
    public override bool IsPolynomial => this is Sqrt && Val.IsPolynomial && Val.PolynomialDegree % 2 == 0;



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
        var expr2 = x/x;
        var expr3 =(5 - 3*c) * Sqrt(16 + c*c);
       Console.WriteLine($"""
        {expr3.ToString()}
        [{string.Join(", ", expr3.Variables)}]
        {expr3.IsConstant}
        {expr3.IsPolynomial}
        {expr3.PolynomialDegree}
        {expr3.Compute(new Dictionary<string, double> { { "x", 2 }, { "y", 2 } })}
        """);
    }
}
