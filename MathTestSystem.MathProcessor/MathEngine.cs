using System.Data;
namespace MathTestSystem.MathProcessor;

    public class MathEngine
{

    public double Evaluate(string expression)
    {
        var dataTable = new DataTable();
        var result = dataTable.Compute(expression, string.Empty);
        return Convert.ToDouble(result);

    }


}
