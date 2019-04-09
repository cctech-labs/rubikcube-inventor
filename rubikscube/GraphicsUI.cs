using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;
using System.Threading.Tasks;

namespace RubikPlugin
{
    class GraphicsUI
    {
        public GraphicsUI(Inventor.Application inInventorApp)
        {
            mInventorApp = inInventorApp;
        }

        public void Start()
        {
            try
            {
                InitClientGraphics();
                DrawCubes();
                UpdateData();
                mInventorApp.ActiveView.Update();
            }
            catch (Exception e)
            {
            }
        }

        public void Stop()
        {
            try
            {
                IsActive = false;
                Running = false;

                ReleaseClientGraphics();
                //mInventeractionEvents.Stop();
                if (null != mSurfaceBodies)
                    for (int i = 0; i < mSurfaceBodies.Count; i++)
                        mSurfaceBodies[i].Delete();

                mInventorApp.ActiveView.Update();

            }
            catch (Exception e)
            {

            }
        }

        private void InitClientGraphics()
        {
            try
            {
                if (mInventorApp.ActiveDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    AssemblyDocument doc = mInventorApp.ActiveDocument as AssemblyDocument;
                    if (null != doc)
                        mClientGraphics = doc.ComponentDefinition.ClientGraphicsCollection.Add("rubikcubeid");
                }
                else if (mInventorApp.ActiveDocumentType == DocumentTypeEnum.kPartDocumentObject)
                {
                    PartDocument doc = mInventorApp.ActiveDocument as PartDocument;
                    if (null != doc)
                        mClientGraphics = doc.ComponentDefinition.ClientGraphicsCollection.Add("rubikcubeid");
                }

                mClientGraphics.Selectable = GraphicsSelectabilityEnum.kAllGraphicsSelectable;
            }
            catch (Exception e)
            {

            }
        }

        private void ReleaseClientGraphics()
        {
            if (null != mClientGraphics)
                mClientGraphics.Delete();
        }

        private void DrawCubes()
        {
            try
            {
                var activeDoc = mInventorApp.ActiveDocument as PartDocument;

                var rubikFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString() + @"\rubik_part.ipt";
                var rubikDoc = mInventorApp.Documents.Open(rubikFile, false);
                PartDocument partDoc = rubikDoc as PartDocument;

                SurfaceBodies surfBodies = partDoc.ComponentDefinition.SurfaceBodies;

                mSurfaceBodies = new List<SurfaceBody>();
                for (int i = 1; i <= surfBodies.Count; i++)
                    mSurfaceBodies.Add(mInventorApp.TransientBRep.Copy(surfBodies[i]));

                // attach IDs to face
                //for (int i = 0; i < mSurfaceBodies.Count; i++)
                //    for (int j = 1; j <= mSurfaceBodies[i].Faces.Count; j++)
                //        mSurfaceBodies[i].Faces[j].AssociativeID = i * 10 + j;

                // iterate over assets and copy them if needed
                for (int i = 1; i <= surfBodies.Count; i++)
                {
                    SurfaceBody surfBody = surfBodies[i];
                    for (int j = 1; j <= surfBody.Faces.Count; j++)
                    {
                        string apperanceName = surfBody.Faces[j].Appearance.DisplayName;
                        try
                        {
                            Asset asset = activeDoc.Assets[apperanceName];
                        }
                        catch (Exception e)
                        {
                            AssetLibrary assetLib = mInventorApp.AssetLibraries["Inventor Material Library"];
                            Asset asset = assetLib.AppearanceAssets[apperanceName];
                            asset = asset.CopyTo(activeDoc);
                        }
                    }
                }

                // create the transient graphics of the cubes
                //mClientGraphics = mInventeractionEvents.InteractionGraphics.OverlayClientGraphics;
                mCubes = new List<GraphicsNode>();
                for (int i = 0; i < mSurfaceBodies.Count; i++)
                {
                    GraphicsNode node = mClientGraphics.AddNode(mClientGraphics.Count + 1);
                    SurfaceGraphics surfGraphics = node.AddSurfaceGraphics(mSurfaceBodies[i]);
                    SurfaceGraphicsFaceList surfFaceList = surfGraphics.DisplayedFaces;
                    for (int j = 1; j <= surfFaceList.Count; j++)
                    {
                        string apperanceName = surfBodies[i + 1].Faces[j].Appearance.DisplayName;
                        if (apperanceName == "Default")
                            apperanceName = "Glossy - Black";

                        surfFaceList[j].Appearance = activeDoc.Assets[apperanceName];
                        surfFaceList[j].Selectable = true;
                    }

                    surfGraphics.ChildrenAreSelectable = true;
                    node.Selectable = true;
                    //node.AllowSlicing = true;
                    mCubes.Add(node);
                }

                SetLights();
                // set the active view to isometric view
                Camera camera = mInventorApp.ActiveView.Camera;
                camera.ViewOrientationType = ViewOrientationTypeEnum.kIsoTopLeftViewOrientation;
                camera.Fit();
                camera.Apply();

                // close the document
                rubikDoc.ReleaseReference();
                rubikDoc.Close(true);

            }
            catch (Exception e)
            {
            }
        }

        private void SetLights()
        {
            PartDocument partDoc = mInventorApp.ActiveDocument as PartDocument;

            LightingStyle lightStyle = partDoc.LightingStyles.Add("RubikAppLight");

            Box box = partDoc.ComponentDefinition.RangeBox;
            for (int i = 0; i < 4; i++)
            {
                Light light = lightStyle.Lights.Add(LightTypeEnum.kModelSpaceLight);
                light.Color = mInventorApp.TransientObjects.CreateColor(255, 241, 224);
                light.Intensity = 0.4;
                light.On = true;
            }

            partDoc.ActiveLightingStyle = lightStyle;
            partDoc.Views[1].Update();
        }

        private void UpdateData()
        {
            Box rubikBox = mInventorApp.TransientGeometry.CreateBox();
            foreach (GraphicsNode node in mCubes)
            {
                Box nodeBox = node.RangeBox;
                //Point max = nodeBox.MaxPoint;
                //Point min = nodeBox.MinPoint;

                //max.TranslateBy(node.Transformation.Translation);
                //min.TranslateBy(node.Transformation.Translation);

                //nodeBox.MaxPoint = max;
                //nodeBox.MinPoint = min;

                rubikBox.Extend(node.RangeBox.MaxPoint);
                rubikBox.Extend(node.RangeBox.MinPoint);
            }

            mCubeArray = new GraphicsNode[3, 3, 3];
            mCubeIJKMap = new Dictionary<GraphicsNode, IJK>();
            foreach (GraphicsNode node in mCubes)
            {
                // find the midpt
                Box nodeBox = node.RangeBox;
                double[] midPt = { (nodeBox.MaxPoint.X + nodeBox.MinPoint.X) / 2.0, 
                                   (nodeBox.MaxPoint.Y + nodeBox.MinPoint.Y) / 2.0, 
                                   (nodeBox.MaxPoint.Z + nodeBox.MinPoint.Z) / 2.0 };

                double[] delta = {nodeBox.MaxPoint.X - nodeBox.MinPoint.X
                                 ,nodeBox.MaxPoint.Y - nodeBox.MinPoint.Y
                                 ,nodeBox.MaxPoint.Z - nodeBox.MinPoint.Z};

                int i = (int)((midPt[0] - rubikBox.MinPoint.X) / delta[0]);
                int j = (int)((midPt[1] - rubikBox.MinPoint.Y) / delta[1]);
                int k = (int)((midPt[2] - rubikBox.MinPoint.Z) / delta[2]);

                mCubeIJKMap.Add(node, new IJK() { I = i, J = j, K = k });
                mCubeArray[i, j, k] = node;
            }

        }

        private void UpdateData(int layer, int direction, int clockwise)
        {
            UpdateData();
        }

        public void Orient(int layer, int direction, int clockwise)
        {
            if (IsActive == true)
                return;

            IsActive = true;
            Running = true;
            if (null == mRotationMatrix)
                mRotationMatrix = mInventorApp.TransientGeometry.CreateMatrix();
            if (null == mAxis)
                mAxis = mInventorApp.TransientGeometry.CreateVector();
            if (null == mRotationPoint)
                mRotationPoint = mInventorApp.TransientGeometry.CreatePoint();

            int instance = 15;

            // get the nodes to be updated
            // x direction
            if (direction == 0)
            {
                // find the mid center cube to find the axis to which cubes are rotated
                GraphicsNode midNode = mCubeArray[layer, 1, 1];

                // find the mid point of cube
                Box nodeBox = midNode.RangeBox;
                double[] midPt = {(nodeBox.MaxPoint.X + nodeBox.MinPoint.X)/2.0
                                 ,(nodeBox.MaxPoint.Y + nodeBox.MinPoint.Y)/2.0
                                 ,(nodeBox.MaxPoint.Z + nodeBox.MinPoint.Z)/2.0};

                double[] axis = { 1 * clockwise, 0, 0 };

                // find the xaxis
                double angle = (Math.PI / (2 * instance));
                mAxis.PutVectorData(ref axis);
                mRotationPoint.PutPointData(ref midPt);
                mRotationMatrix.SetToIdentity();
                mRotationMatrix.SetToRotation(angle, mAxis, mRotationPoint);

                for (int i = 1; i <= instance; i++)
                {
                    for (int y = 0; y < 3; y++)
                        for (int z = 0; z < 3; z++)
                        {
                            if (Running)
                            {
                                Matrix nodeMatrix = mCubeArray[layer, y, z].Transformation;
                                nodeMatrix.PreMultiplyBy(mRotationMatrix);
                                mCubeArray[layer, y, z].Transformation = nodeMatrix;
                            }
                        }
                    mInventorApp.ActiveView.Update();
                    mInventorApp.UserInterfaceManager.DoEvents();
                }
            }
            // y direction
            else if (direction == 1)
            {
                // find the mid center cube to find the axis to which cubes are rotated
                GraphicsNode midNode = mCubeArray[1, layer, 1];

                // find the mid point of cube
                Box nodeBox = midNode.RangeBox;
                double[] midPt = {(nodeBox.MaxPoint.X + nodeBox.MinPoint.X)/2.0
                                 ,(nodeBox.MaxPoint.Y + nodeBox.MinPoint.Y)/2.0
                                 ,(nodeBox.MaxPoint.Z + nodeBox.MinPoint.Z)/2.0};

                double[] axis = { 0, 1 * clockwise, 0 };

                // find the xaxis
                double angle = (Math.PI / (2 * instance));
                mAxis.PutVectorData(ref axis);
                mRotationPoint.PutPointData(ref midPt);
                mRotationMatrix.SetToIdentity();
                mRotationMatrix.SetToRotation(angle, mAxis, mRotationPoint);

                for (int i = 1; i <= instance; i++)
                {
                    for (int x = 0; x < 3; x++)
                        for (int z = 0; z < 3; z++)
                        {
                            if (Running)
                            {
                                Matrix nodeMatrix = mCubeArray[x, layer, z].Transformation;
                                nodeMatrix.PreMultiplyBy(mRotationMatrix);
                                mCubeArray[x, layer, z].Transformation = nodeMatrix;
                            }
                        }
                    mInventorApp.ActiveView.Update();
                    mInventorApp.UserInterfaceManager.DoEvents();
                }
            }
            // z direction
            else if (direction == 2)
            {
                // find the mid center cube to find the axis to which cubes are rotated
                GraphicsNode midNode = mCubeArray[1, 1, layer];

                // find the mid point of cube
                Box nodeBox = midNode.RangeBox;
                double[] midPt = {(nodeBox.MaxPoint.X + nodeBox.MinPoint.X)/2.0
                                 ,(nodeBox.MaxPoint.Y + nodeBox.MinPoint.Y)/2.0
                                 ,(nodeBox.MaxPoint.Z + nodeBox.MinPoint.Z)/2.0};

                double[] axis = { 0, 0, 1 * clockwise };

                // find the xaxis
                double angle = (Math.PI / (2 * instance));
                mAxis.PutVectorData(ref axis);
                mRotationPoint.PutPointData(ref midPt);
                mRotationMatrix.SetToIdentity();
                mRotationMatrix.SetToRotation(angle, mAxis, mRotationPoint);

                for (int i = 1; i <= instance; i++)
                {
                    for (int x = 0; x < 3; x++)
                        for (int y = 0; y < 3; y++)
                        {
                            if (Running)
                            {
                                Matrix nodeMatrix = mCubeArray[x, y, layer].Transformation;
                                nodeMatrix.PreMultiplyBy(mRotationMatrix);
                                mCubeArray[x, y, layer].Transformation = nodeMatrix;
                            }
                        }
                    //System.Windows.Forms.Application.DoEvents();
                    mInventorApp.ActiveView.Update();
                    mInventorApp.UserInterfaceManager.DoEvents();
                }
            }

            if (Running)
                UpdateData();
            IsActive = false;
        }

        private Application mInventorApp;
        private ClientGraphics mClientGraphics;

        private List<GraphicsNode> mCubes;
        private List<SurfaceBody> mSurfaceBodies;
        private GraphicsNode[, ,] mCubeArray;
        private Dictionary<GraphicsNode, IJK> mCubeIJKMap;

        // temp variables
        private Matrix mRotationMatrix;
        private Vector mAxis;
        private Point mRotationPoint;
        public bool Running;

        public bool IsActive;
    }

    struct IJK
    {
        public int I { get; set; }
        public int J { get; set; }
        public int K { get; set; }
    }
}
