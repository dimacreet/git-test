using System;
using System.Collections.Generic;
using System.Linq;

public class Normilize : InputBasedIndicator
{
    private IRIndex<double> input;
    private int period;
    public Normilize(IRIndex<double> input, int period)
    {
        this.input = input;
        this.period = period;
        input.NewDataAction(Recalc);
    }

    public void Recalc(double c)
    {
        var min_c = Math.Min(input.Count, period);
        var max = -99999e10;
        var min = 99999e10;

        for (var i = 0; i < min_c; i++)
        {
            if (input[-i] > max) max = input[-i];
            if (input[-i] < min) min = input[-i];
        }
            vals.Add((input-min)/(max-min));
    }
}



