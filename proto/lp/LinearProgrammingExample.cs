using System;

using CenterSpace.NMath.Core;
using CenterSpace.NMath.Analysis;

namespace CenterSpace.NMath.Analysis.Examples.CSharp
{
  class LinearProgrammingExample
  {
    /// <summary>
    /// A .NET example in C# showing how to solve a linear system using linear programming and
    /// the simplex method.
    /// </summary>
    static void Main(string[] args)
    {
      DoubleVector revenue = new DoubleVector(0, 0, 5, 10);

      int numConstraints = 3;
      int numVars = 4;
      DoubleMatrix constraints = new DoubleMatrix(numConstraints, numVars);
      DoubleVector rightHandSides = new DoubleVector(constraints.Rows);
      constraints[0, Slice.All] = new DoubleVector(-5, 0, 5, 10);
      rightHandSides[0] = 0.0;
      constraints[1, Slice.All] = new DoubleVector(0, -10, 5, 10);
      rightHandSides[1] = 0.0;
      constraints[2, Slice.All] = new DoubleVector(1, 1, 0, 0);
      rightHandSides[2] = 1.0;



      // Create an LP solver with an error tolerance of 0.001.
      SimplexLPSolver solver = new SimplexLPSolver(0.001);

      solver.Solve(revenue, constraints, rightHandSides, 2, 0, 1);

      Console.WriteLine("F: {0}", revenue);
      Console.WriteLine("C: {0}", constraints);
      Console.WriteLine("R: {0}", rightHandSides);

      // Was a finite solution found?
      Console.WriteLine();
      if (solver.IsGood)
      {
        Console.WriteLine("solution: {0}", solver.Solution);
        Console.WriteLine();
        Console.WriteLine("optimal value: {0}",  solver.OptimalValue);
      }
      else
      {
          Console.WriteLine("Solution is bad, status {0}", solver.Status.ToString());
      }
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("Press Enter Key");
      Console.Read();



    }  // Main

  }  // class

} // namespace
