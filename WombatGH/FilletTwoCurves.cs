﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using Rhino.Input;
using Rhino.DocObjects;
using Rhino.Input.Custom;


using System.Drawing;
using WombatGH.Properties;

namespace WombatGH
{
    public class FilletTwoCurves : GH_Component
    {
        public FilletTwoCurves()
          : base("FilletTwoCurves", "FilletTwoCurves",
              "FilletTwoCurves",
              "Wombat", "Curve")
        {
        }

        protected override Bitmap Icon => Resources.WombatGH_FilletTwoCurves;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("First Curve", "C", "Fisrt Curves.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Second Curve", "C", "Second Curves.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "R", "Fillet Radius.", GH_ParamAccess.item,0);
            pManager.AddBooleanParameter("Join","J","Join.",GH_ParamAccess.item,true);
            pManager.AddBooleanParameter("Arc Extension", "A", "ArcExtension.", GH_ParamAccess.item,true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve1 Trimmed", "C1", "Curve1 Trimmed.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve2 Trimmed", "C2", "Curve2 Trimmed.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Filleted Curve", "CF", "Filleted Curve.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve Joined", "CJ", "Joined curve.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> crv1 = new List<Curve>();
            List<Curve> crv2 = new List<Curve>();
            double radius = 0;
            bool join = true;
            bool trim = true;
            bool arcExtension = true;
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            double angelTol = Rhino.RhinoDoc.ActiveDoc.ModelAngleToleranceDegrees;

            if (!DA.GetDataList(0, crv1)) return;
            if (!DA.GetDataList(1, crv2)) return;
            if (!DA.GetData<double>(2, ref radius)) return;
            if (!DA.GetData<bool>(3, ref join)) return;
            if (!DA.GetData<bool>(4, ref arcExtension)) return;

            List<Curve> joinedFilletOutput = new List<Curve>();
            List<Curve> crv1Output = new List<Curve>();
            List<Curve> crv2Output = new List<Curve>();
            List<Curve> filletOutput = new List<Curve>();

            if (crv2.Count != crv1.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of geometry in Curve list 1 and Curve list 2 are unequal.");
                return;
            }

            for (int i = 0; i < crv1.Count; i++)
            {
                if (crv1 != null && crv2 != null)
                {
                    var crvEndPt1 = crv1[i].PointAt(0);
                    var crvEndPt2 = crv2[i].PointAt(0);
                    Curve[] filletCurves = Curve.CreateFilletCurves(crv1[i], crvEndPt1, crv2[i], crvEndPt2, radius, join, trim, arcExtension, tol, angelTol);

                    if (filletCurves.Length == 0 )
                    {
                        if (arcExtension == false)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to extend curve2 to curve1. No filleted geometry produced.");
                            return;
                        }
                        else if (arcExtension == true)
                        {
                            if (Curve.CreateFilletCurves(crv1[i], crvEndPt1, crv2[i], crvEndPt2, radius, join, trim, false, tol, angelTol).Length == 0)
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to extend curve2 to curve1. No filleted geometry produced.");
                                return;
                            }
                            else
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to create arc extension. No filleted geometry produced");
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (join)
                        {
                            joinedFilletOutput.Add(filletCurves[0]);
                        }
                        else
                        {
                            if (radius == 0)
                            {
                                crv1Output.Add(filletCurves[0]);
                                crv2Output.Add(filletCurves[1]);
                            }
                            else 
                            {
                                crv1Output.Add(filletCurves[0]);
                                crv2Output.Add(filletCurves[1]);
                                filletOutput.Add(filletCurves[2]);
                            }
                        }

                        DA.SetDataList("Curve Joined", joinedFilletOutput);
                        DA.SetDataList("Curve1 Trimmed", crv1Output);
                        DA.SetDataList("Curve2 Trimmed", crv2Output);
                        DA.SetDataList("Filleted Curve", filletOutput);
                    }
                }
                else
                {
                    return;
                }
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("4ecd8664-7e01-4b01-9313-a72d48fdd1e9"); }
        }
    }
}