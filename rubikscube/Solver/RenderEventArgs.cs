/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RubikSolver
{
	class RenderEventArgs: EventArgs
	{
		public RenderInfo RenderInfo { get; private set; }
		public RenderEventArgs(RenderInfo renderInfo)
		{
			RenderInfo = renderInfo;
		}
	}
}
