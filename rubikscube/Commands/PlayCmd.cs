/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;

namespace RubikPlugin.Commands
{
    class PlayCmd
    {
        public delegate void PlayEvent(int layer, int direction, int clockwise);
        public delegate void Invalidate();

        public PlayCmd(Inventor.Application inInventorApp)
        {
            mInventorApp = inInventorApp;
            mZAxis = mInventorApp.TransientGeometry.CreateVector(0, 0, 1);
        }

        public void OnClick()
        {
            if (null != OnInvalidateEvent)
                OnInvalidateEvent();

            mMouseDown = false;

            //mInventorApp.CommandManager.UserInputEvents.OnPreSelect += new UserInputEventsSink_OnPreSelectEventHandler(OnUserInputPreSelect);
            mInteractionEvents = mInventorApp.CommandManager.CreateInteractionEvents();

            mSelectEvents = mInteractionEvents.SelectEvents;
            mSelectEvents.AddSelectionFilter(SelectionFilterEnum.kAllEntitiesFilter);
            mSelectEvents.SingleSelectEnabled = true;
            mSelectEvents.OnPreSelect += new SelectEventsSink_OnPreSelectEventHandler(OnPreSelect);
            mSelectEvents.OnSelect += new SelectEventsSink_OnSelectEventHandler(OnSelect);

            mMouseEvents = mInteractionEvents.MouseEvents;
            mMouseEvents.MouseMoveEnabled = true;
            mMouseEvents.OnMouseDown += new MouseEventsSink_OnMouseDownEventHandler(OnMouseDown);
            mMouseEvents.OnMouseUp += new MouseEventsSink_OnMouseUpEventHandler(OnMouseUp);
            mMouseEvents.OnMouseMove += new MouseEventsSink_OnMouseMoveEventHandler(OnMouseMove);

            mInteractionEvents.Start();
        }

        public void EnableInteration(bool status)
        {
            if (null != mInteractionEvents)
            {
                if (status)
                    mInteractionEvents.Start();
                else
                    mInteractionEvents.Stop();
            }
        }

        private void OnMouseDown(MouseButtonEnum Button, ShiftStateEnum ShiftKeys, Point ModelPosition, Point2d ViewPosition, View View)
        {
            if (Button == MouseButtonEnum.kLeftMouseButton)
            {
                mMouseDown = true;
                mLastPos = mInventorApp.TransientGeometry.CreatePoint(ModelPosition.X, ModelPosition.Y, ModelPosition.Z);
                mCurPos = mInventorApp.TransientGeometry.CreatePoint(ModelPosition.X, ModelPosition.Y, ModelPosition.Z);
            }
        }

        private void OnMouseUp(MouseButtonEnum Button, ShiftStateEnum ShiftKeys, Point ModelPosition, Point2d ViewPosition, View View)
        {
            if (Button == MouseButtonEnum.kLeftMouseButton)
            {
                mMouseDown = false;
                mSelectedFace = null;

                mCurPos.PutPointData(new double[] { 0, 0, 0 });
                mLastPos.PutPointData(new double[] { 0, 0, 0 });
                mRadius = 0;
            }
        }

        private void OnMouseMove(MouseButtonEnum Button, ShiftStateEnum ShiftKeys, Point ModelPosition, Point2d ViewPosition, View View)
        {
            if (Button == MouseButtonEnum.kLeftMouseButton && mMouseDown == true && null != mSelectedFace)
            {
                mCurPos.X = ModelPosition.X;
                mCurPos.Y = ModelPosition.Y;
                mCurPos.Z = ModelPosition.Z;

                double[] del = { mCurPos.X - mLastPos.X, mCurPos.Y - mLastPos.Y, mCurPos.Z - mLastPos.Z };
                if (Math.Abs(del[0]) > 0.0001 || Math.Abs(del[1]) > 0.0001 || Math.Abs(del[2]) > 0.0001)
                {
                    int layer = -1, direction = -1, clockwise = -1;
                    GetLayer(del, ref layer, ref direction, ref clockwise);

                    if (null != OnPlayEvent)
                        OnPlayEvent(layer, direction, clockwise);
                    mSelectedFace = null;
                }
                mLastPos.X = mCurPos.X;
                mLastPos.Y = mCurPos.Y;
                mLastPos.Z = mCurPos.Z;
            }
        }

        private void OnUserInputPreSelect(ref object PreSelectEntity, out bool DoHighlight, ref ObjectCollection MorePreSelectEntities, SelectionDeviceEnum SelectionDevice, Point ModelPosition, Point2d ViewPosition, View View)
        {
            if (PreSelectEntity is SurfaceGraphicsFace)
                DoHighlight = true;
            else
                DoHighlight = false;
        }

        private void OnPreSelect(ref object PreSelectEntity, out bool DoHighlight, ref Inventor.ObjectCollection MorePreSelectEntities, Inventor.SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Inventor.Point2d ViewPosition, Inventor.View View)
        {
            if (PreSelectEntity is SurfaceGraphicsFace)
                DoHighlight = true;
            else
                DoHighlight = false;
        }

        private void OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Point ModelPosition, Point2d ViewPosition, View View)
        {
            if (JustSelectedEntities.Count == 0)
                return;

            SurfaceGraphicsFace face = JustSelectedEntities[1] as SurfaceGraphicsFace;
            if (null != face)
                mSelectedFace = face;

            if (mRadius < 0.0001)
                CalculateRadius();
            mSelectEvents.ResetSelections();
        }

        private double[] PointOnPlaneUsingRay(Point rayPt, Vector rayDir, Plane plane)
        {
            Vector normal = plane.Normal.AsVector();
            double d = (plane.RootPoint.VectorTo(rayPt).DotProduct(normal)) / (rayDir.DotProduct(normal));

            return new double[] { rayPt.X + d * rayDir.X, rayPt.Y + d * rayDir.Y, rayPt.Z + d * rayDir.Z };
        }

        private void CalculateRadius()
        {
            if (null != mSelectedFace)
            {
                Plane plane = mSelectedFace.Face.Geometry as Plane;
                if (null != plane)
                {
                    Box planeBox = plane.Evaluator.RangeBox;
                    mRadius = planeBox.MaxPoint.DistanceTo(planeBox.MinPoint) * 1.5;
                }
            }
        }

        private void GetLayer(double[] del, ref int Layer, ref int direction, ref int clockwise)
        {
            Box sceneBox = GetSceneBox();
            if (null != sceneBox && null != mSelectedFace)
            {
                GraphicsNode node = mSelectedFace.Parent.Parent;

                Box nodeBox = node.RangeBox;
                double[] midPt = { (nodeBox.MaxPoint.X + nodeBox.MinPoint.X) / 2.0, 
                                   (nodeBox.MaxPoint.Y + nodeBox.MinPoint.Y) / 2.0, 
                                   (nodeBox.MaxPoint.Z + nodeBox.MinPoint.Z) / 2.0 };

                double[] delta = {nodeBox.MaxPoint.X - nodeBox.MinPoint.X
                                 ,nodeBox.MaxPoint.Y - nodeBox.MinPoint.Y
                                 ,nodeBox.MaxPoint.Z - nodeBox.MinPoint.Z};

                int i = (int)((midPt[0] - sceneBox.MinPoint.X) / delta[0]);
                int j = (int)((midPt[1] - sceneBox.MinPoint.Y) / delta[1]);
                int k = (int)((midPt[2] - sceneBox.MinPoint.Z) / delta[2]);

                Plane plane = mSelectedFace.Face.Geometry as Plane;
                if (null == plane)
                    return;

                UnitVector normal = plane.Normal;
                if (mSelectedFace.Face.IsParamReversed == true)
                {
                    normal.X = -normal.X;
                    normal.Y = -normal.Y;
                    normal.Z = -normal.Z;
                }
                normal.TransformBy(node.Transformation);
                if (Math.Abs(Math.Abs(normal.X) - 1.0) < 0.0001)
                {
                    if (Math.Abs(del[1]) > Math.Abs(del[2]))
                    {
                        Layer = k;
                        direction = 2;
                        //clockwise = (del[1] > 0 ? 1 : -1) * (int)normal.X;
                        clockwise = (del[1] * normal.X > 0 ? 1 : -1);
                    }
                    else
                    {
                        Layer = j;
                        direction = 1;
                        //clockwise = (del[2] > 0 ? -1 : 1) * (int)normal.X;
                        clockwise = (del[2] * normal.X > 0 ? -1 : 1);
                    }
                }
                else if (Math.Abs(Math.Abs(normal.Y) - 1.0) < 0.0001)
                {
                    if (Math.Abs(del[2]) > Math.Abs(del[0]))
                    {
                        Layer = i;
                        direction = 0;
                        //clockwise = (del[2] > 0 ? 1 : -1) * (int)normal.Y;
                        clockwise = (del[2] * normal.Y > 0 ? 1 : -1);
                    }
                    else
                    {
                        Layer = k;
                        direction = 2;
                        //clockwise = (del[0] > 0 ? -1 : 1) * (int)normal.Y;
                        clockwise = (del[0] * normal.Y > 0 ? -1 : 1);
                    }
                }
                else if (Math.Abs(Math.Abs(normal.Z) - 1.0) < 0.0001)
                {
                    if (Math.Abs(del[0]) > Math.Abs(del[1]))
                    {
                        Layer = j;
                        direction = 1;
                        //clockwise = (del[0] > 0 ? 1 : -1) * (int)normal.Z;
                        clockwise = (del[0] * normal.Z > 0 ? 1 : -1);
                    }
                    else
                    {
                        Layer = i;
                        direction = 0;
                        //clockwise = (del[1] > 0 ? -1 : 1) * (int)normal.Z;
                        clockwise = (del[1] * normal.Z > 0 ? -1 : 1);
                    }
                }
            }
        }

        private Box GetSceneBox()
        {
            if (null == mSceneBox)
            {
                PartDocument partDoc = mInventorApp.ActiveDocument as PartDocument;
                if (null != partDoc)
                {
                    ClientGraphics clientGraphics = null;
                    try
                    {
                        clientGraphics = partDoc.ComponentDefinition.ClientGraphicsCollection["rubikcubeid"];
                    }
                    catch (Exception)
                    {
                    }

                    if (null != clientGraphics)
                    {
                        mSceneBox = mInventorApp.TransientGeometry.CreateBox();
                        foreach (GraphicsNode node in clientGraphics)
                        {
                            mSceneBox.Extend(node.RangeBox.MinPoint);
                            mSceneBox.Extend(node.RangeBox.MaxPoint);
                        }
                    }
                }

            }

            return mSceneBox;
        }

        public event PlayEvent OnPlayEvent;
        public event Invalidate OnInvalidateEvent;

        private Application mInventorApp;
        private InteractionEvents mInteractionEvents;
        private SelectEvents mSelectEvents;
        private MouseEvents mMouseEvents;

        private Vector mZAxis;
        private double mRadius;

        private Box mSceneBox;

        private SurfaceGraphicsFace mSelectedFace;
        private Point mLastPos, mCurPos;
        private bool mMouseDown;
    }
}
