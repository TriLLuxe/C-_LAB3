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
    public static Expr Sin(Expr operand) => new Sin(operand);
    public static Expr Cos(Expr operand) => new Cos(operand);
    public static Expr Tan(Expr operand) => new Tan(operand);
    public static Expr Ctg(Expr operand) => new Ctg(operand);

    // Унарные операторы
    public static Expr operator +(Expr operand) => new UnaryPlus(operand);
    public static Expr operator -(Expr operand) => new UnaryMinus(operand);
    // Бинарные операторы
    public static Expr operator +(Expr a, Expr b) => new BinaryAddition(a, b);
    public static Expr operator -(Expr a, Expr b) => new BinarySubtraction(a, b);
    public static Expr operator *(Expr a, Expr b) => new Multiplication(a, b);
    public static Expr operator /(Expr a, Expr b) => new Division(a, b);
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
    public override string ToString() 
    {
        string operandStr = Operand?.ToString() ?? string.Empty;
        if (operandStr.StartsWith("-"))
        {
            return operandStr.Substring(1); 
        }
        else
        {
            return operandStr;
        }
    }
}

public class UnaryMinus : UnaryOperation
{
    public UnaryMinus(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => -Operand.Compute(variableValues);
    public override string ToString() 
    {
        string operandStr = Operand?.ToString() ?? string.Empty;
        if (operandStr.StartsWith("-"))
        {
            return operandStr.Substring(1);
        }
        else
        {
            return $"(-{operandStr})";
        }
    }
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
}

public class BinaryAddition : BinaryOperation
{
    public BinaryAddition(Expr left, Expr right) : base(left, right) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) + B.Compute(variableValues);
    public override bool IsPolynomial => A.IsPolynomial && B.IsPolynomial;
    public override int PolynomialDegree
    {
        get
        {
            if(!A.IsPolynomial||!B.IsPolynomial) return 0;
            else return Math.Max(A.PolynomialDegree, B.PolynomialDegree);
        }
    }
   //  public override string ToString() => $"({A} + {B})";
   public override string ToString()
{
    // Если правый операнд это отрицательная константа
    if (B is Constant constant && constant.Value < 0)
    {
        // Если выражение вида (x + (-1)), заменяем на (x - 1)
        return $"({A} - {Math.Abs(constant.Value)})";
    }

    // В других случаях просто выводим обычное выражение
    return $"({A} + {B})";
}
}

public class BinarySubtraction : BinaryOperation 
{
    public BinarySubtraction(Expr a, Expr b) : base(a, b) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) - B.Compute(variableValues);
    public override bool IsPolynomial => A.IsPolynomial && B.IsPolynomial;
    public override int PolynomialDegree => Math.Max(A.PolynomialDegree, B.PolynomialDegree);

   //  public override string ToString() => $"({A} - {B})";
  public override string ToString()
{
    // Если правый операнд это отрицательная константа
    if (B is Constant constant && constant.Value < 0)
    {
        // Если выражение вида (x - (-1)), заменяем на (x + 1)
        return $"({A} + {Math.Abs(constant.Value)})";
    }

    
    if (A is Constant constantA && constantA.Value < 0)
    {
        
        return $"{A} - {B}";
    }

    // В других случаях просто выводим обычное выражение
    return $"({A} - {B})";
}
}

public class Multiplication : BinaryOperation
{
    public Multiplication(Expr a, Expr b) : base(a, b) { }
    public override bool IsPolynomial => A.IsPolynomial||B.IsPolynomial;

    public override int PolynomialDegree => A.PolynomialDegree + B.PolynomialDegree;
    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => A.Compute(variableValues) * B.Compute(variableValues);
    //  public override string ToString() => $"({A} * {B})";
    public override string ToString()
{
    // Если правый операнд это отрицательная константа
    if (B is Constant constant2 && constant2.Value < 0)
    {
        
        return $"{A} * {B}";
    }

    // Если левый операнд это отрицательная константа
    if (A is Constant constant1 && constant1.Value < 0)
    {
        
        return $"{A} * {B}";
    }

    
    return $"{A} * {B}";
}
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
    public override int PolynomialDegree 
    {
    get
      {
        if(!IsPolynomial) return 0;
        else if (A.IsPolynomial && B.IsPolynomial)
        {
            int numeratorDegree = A.PolynomialDegree;
            int denominatorDegree = B.PolynomialDegree;
            if (numeratorDegree < denominatorDegree) 
                return 0;
            bool divides = divideCheck(A,B);
            if (divides) return numeratorDegree-denominatorDegree;
            else return 0;
        }
        else return 0;
      }
   }
    private bool divideCheck(Expr numerator, Expr denominator)
    {
        int denominatorDegree = denominator.PolynomialDegree;
        if (numerator is BinaryAddition || numerator is BinarySubtraction) 
        {
            var binaryNumerator = (BinaryOperation)numerator;
            return divideCheck(binaryNumerator.A, denominator) && divideCheck(binaryNumerator.B, denominator);
        }
        
        if (numerator.PolynomialDegree >= denominatorDegree)
        {
            return true;
        }
        return false; 
    }

    public override string ToString() => $"({A} / {B})";
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
        else throw new ArgumentException($"{Name} is not defined.");
    }

    public override string ToString() => Name;
}

public abstract class Function : Expr
{
    public Expr Val { get; }
    public Function(Expr val) => Val = val;
    public override IEnumerable<string> Variables => Val.Variables;
    public override bool IsConstant => true;
    public override int PolynomialDegree => 0;
     public override bool IsPolynomial
    {
        get
        {
            if(Val.IsConstant) return true;
            else return false;
        }
    }
}

public class Sqrt : Function
{
    public Sqrt(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues)
    {
        double operandValue = Val.Compute(variableValues);
        if (operandValue < 0)
            throw new ArgumentException("Квадратный корень из отрицательного числа не допускается.");
        return Math.Sqrt(operandValue);
    }
   

    public override string ToString() => $"Sqrt({Val})";
}

public class Sin : Function
{
    public Sin(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Sin(Val.Compute(variableValues));
    public override string ToString() => $"Sin({Val})";
}

public class Cos : Function
{
    public Cos(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Cos(Val.Compute(variableValues));
    public override string ToString() => $"Cos({Val})";
}

public class Tan : Function
{
    public Tan(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Tan(Val.Compute(variableValues));
    public override string ToString() => $"Tan({Val})";
}

public class Ctg : Function
{
    public Ctg(Expr operand) : base(operand) { }

    public override double Compute(IReadOnlyDictionary<string, double> variableValues) => 1 / Math.Tan(Val.Compute(variableValues));
    public override string ToString() => $"Ctg({Val})";
}

class Program
{
    static void Main(string[] args)
    {
        // Тестирование
        var x = new Variable("x");
        var y = new Variable("y");
        var c = new Constant(3);
       var expr1 = (x-x);
       
        Console.WriteLine($"""
       
        
        
        """);
    }
}
