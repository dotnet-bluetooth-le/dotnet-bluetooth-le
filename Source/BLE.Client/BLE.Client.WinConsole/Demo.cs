using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record Demo
(
    string Description,
    Func<Task> Method
);

