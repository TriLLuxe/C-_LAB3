using Xunit;
using System.Collections.Generic;

public class ExprTests
{
    [Fact]
    public void TestSimplification()
    {
        var x = new Variable("x");

        var expr1 = x - x; // Должно упроститься до 0
        Assert.Equal("0", expr1.ToString());
        Assert.True(expr1.IsConstant);
        Assert.True(expr1.IsPolynomial);
        Assert.Equal(0, expr1.PolynomialDegree);

        var expr2 = x * 1; // Должно упроститься до x
        Assert.Equal("x", expr2.ToString());

        var expr4 = x + 0; // Должно упроститься до x
        Assert.Equal("x", expr4.ToString());

        var expr5 = 0 + x; // Должно упроститься до x
        Assert.Equal("x", expr5.ToString());

        var expr6 = x * 0; // Должно упроститься до 0
        Assert.Equal("0", expr6.ToString());

        var expr7 = 0 * x; // Должно упроститься до 0
        Assert.Equal("0", expr7.ToString());

        var expr8 = x / x; // Должно упроститься до 1
        Assert.Equal("1", expr8.ToString());
        
    }

    [Fact]
    public void TestFunction()
    {
        var x = new Variable("x");

        var expr10 = Expr.Sin(x); // Синус от x
        Assert.Equal("Sin(x)", expr10.ToString());
        Assert.False(expr10.IsConstant);
        Assert.False(expr10.IsPolynomial);
        Assert.Equal(0, expr10.PolynomialDegree);
        

    }
    [Fact]
    public void TestComplexExpression()
    {
        var x = new Variable("x");

        // Выражение (2*x*x - 2) / (x*x - 1)
        var expr = ((2 * x * x) - 2) / (x * x - 1);

        // Ожидаем, что выражение упростится до "2" при условии, что x != ±1
        // Проверка строки
        Assert.Equal("2", expr.ToString());

        // Проверка вычисления значения, где x ≠ ±1
        var values = new Dictionary<string, double> { { "x", 3 } };
        Assert.Equal(2, expr.Compute(values));

        // Проверка для других значений x
        values["x"] = 5;
        Assert.Equal(2, expr.Compute(values));
    }

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
