using System;
using System.Collections.Generic;
using System.Linq;

public class Fisher : InputBasedIndicator
{
    private IRIndex<double> input;
    public Fisher(IRIndex<double> input)
    {
        this.input = input;
        input.NewDataAction(Recalc);

    }

    public void Recalc(double c)
    {
        vals.Add((Math.Exp(2*input)-1)/(Math.Exp(2*input)+1));
    }
}



