using System;
using Xunit;
using System.Collections.Generic;

public class ExprTests
{
    [Fact]
public void TestIsConstant1()
{
    var x = new Variable("x");
    var y = new Variable("y");
    var expr1 = (x - 4) * (3 * x + y * y) / 5;
    Assert.False(expr1.IsConstant);
}

[Fact]
public void TestIsPolynomial1()
{
    var x = new Variable("x");
    var y = new Variable("y");
    var expr1 = (x - 4) * (3 * x + y * y) / 5;
    Assert.True(expr1.IsPolynomial);
}

[Fact]
public void TestPolynomialDegree1()
{
    var x = new Variable("x");
    var y = new Variable("y");
    var expr1 = (x - 4) * (3 * x + y * y) / 5;
    Assert.Equal(3, expr1.PolynomialDegree);
}

[Fact]
public void TestComputeResult1()
{
    var x = new Variable("x");
    var y = new Variable("y");
    var expr1 = (x - 4) * (3 * x + y * y) / 5;
    var values = new Dictionary<string, double> { { "x", 1 }, { "y", 2 } };
    Assert.Equal(-4.2, expr1.Compute(values));
}

   [Fact]
public void TestIsConstant2()
{
    var c = new Constant(3);
    var expr2 = (5 - 3 * c) * Expr.Sqrt(16 + c * c);
    Assert.True(expr2.IsConstant);
}

[Fact]
public void TestIsPolynomial2()
{
    var c = new Constant(3);
    var expr2 = (5 - 3 * c) * Expr.Sqrt(16 + c * c);
    Assert.True(expr2.IsPolynomial);
}

[Fact]
public void TestPolynomialDegree2()
{
    var c = new Constant(3);
    var expr2 = (5 - 3 * c) * Expr.Sqrt(16 + c * c);
    Assert.Equal(0, expr2.PolynomialDegree);
}

[Fact]
public void TestComputeResult2()
{
    var c = new Constant(3);
    var expr2 = (5 - 3 * c) * Expr.Sqrt(16 + c * c);
    var values = new Dictionary<string, double> { { "x", 1 }, { "y", 2 } };
    Assert.Equal(-20, expr2.Compute(values));
}
    [Fact]
    public void TestFunction()
    {
        var x = new Variable("x");

        var expr10 = Expr.Sin(x); // Синус от x
        Assert.Equal("Sin(x)", expr10.ToString());
        Assert.True(expr10.IsConstant);
        Assert.True(expr10.IsPolynomial);
        Assert.Equal(0, expr10.PolynomialDegree);
        

    }
    // [Fact]
    // public void TestComplexExpression()
    // {
    //     var x = new Variable("x");

    //     // Выражение (2*x*x - 2) / (x*x - 1)
    //     var expr = ((2 * x * x) - 2) / (x * x - 1);

    //     // Ожидаем, что выражение упростится до "2" при условии, что x != ±1
    //     // Проверка строки
    //     Assert.Equal("2", expr.ToString());
    //     Assert.True( expr.IsConstant);
    //     Assert.True( expr.IsPolynomial);
    //     Assert.Equal(2, expr.PolynomialDegree);

    //     // Проверка вычисления значения, где x ≠ ±1
    //     var values = new Dictionary<string, double> { { "x", 3 } };
    //     Assert.Equal(2, expr.Compute(values));

    //     // Проверка для других значений x
    //     values["x"] = 5;
    //     Assert.Equal(2, expr.Compute(values));
    // }

    [Fact]
    public void TestConstantAndVariable()
    {
        var x = new Variable("x");
        var c = new Constant(3);

        Assert.False(x.IsConstant);
        Assert.True(x.IsPolynomial);
        Assert.Equal(1, x.PolynomialDegree);

        Assert.True(c.IsConstant);
        Assert.True(c.IsPolynomial);
        Assert.Equal(0, c.PolynomialDegree);
    }

    [Fact]
    public void TestCompute()
    {
        var x = new Variable("x");
        var y = new Variable("y");
        var expr = x + y;

        var values = new Dictionary<string, double>
        {
            { "x", 2 },
            { "y", 3 }
        };

        Assert.Equal(5, expr.Compute(values));
    }
}
