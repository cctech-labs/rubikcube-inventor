﻿/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;

namespace RubikSolver
{
  class Point3D
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Point3D(double x, double y, double z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    public Point3D Clone()
    {
      Point3D newPoint = new Point3D(X, Y, Z);
      return newPoint;
    }

    public void Rotate(RotationType type, double angle)
    {
      double rad = angle * Math.PI / 180;
      double cosa = Math.Cos(rad);
      double sina = Math.Sin(rad);

      Point3D old = new Point3D(X, Y, Z);

      switch (type)
      {
        case RotationType.X:
          Y = old.Y * cosa - old.Z * sina;
          Z = old.Y * sina + old.Z * cosa;
          break;
        case RotationType.Y:
          X = old.Z * sina + old.X * cosa;
          Z = old.Z * cosa - old.X * sina;
          break;
        case RotationType.Z:
          X = old.X * cosa - old.Y * sina;
          Y = old.X * sina + old.Y * cosa;
          break;
      }
    }

    public Point3D Project(int viewWidth, int viewHeight, int fov, int viewDistance, double scale)
    {
      double factor = fov / (viewDistance + this.Z) * scale;
      double Xn = this.X * factor + viewWidth / 2;
      double Yn = this.Y * factor + viewHeight / 2;
      return new Point3D(Xn, Yn, this.Z);
    }

    public enum RotationType { X, Y, Z }

  }
}