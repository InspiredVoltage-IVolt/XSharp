﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSharp.Tokens {
  public class Register16 : RegisterBase {
    public static readonly string[] Names = "AX,BX,CX,DX".Split(',');

    public Register16() {
      Size = 16;
    }

    protected override bool IsMatch(string aText) {
      return Names.Contains(aText.ToUpper());
    }
  }
}