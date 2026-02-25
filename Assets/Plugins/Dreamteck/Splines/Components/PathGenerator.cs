using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Path Generator")]
    public class PathGenerator : MeshGenerator
    {
        [HideInInspector] public float _thickness = 10f;

        public int slices
        {
            get { return _slices; }
            set
            {
                if (value != _slices)
                {
                    if (value < 1) value = 1;
                    _slices = value;
                    Rebuild();
                }
            }
        }

        public bool useShapeCurve
        {
            get { return _useShapeCurve; }
            set
            {
                if (value != _useShapeCurve)
                {
                    _useShapeCurve = value;
                    if (_useShapeCurve)
                    {
                        _shape = new AnimationCurve();
                        _shape.AddKey(new Keyframe(0, 0));
                        _shape.AddKey(new Keyframe(1, 0));
                    } else _shape = null;
                    Rebuild();
                }
            }
        }

        public bool compensateCorners
        {
            get { return _compensateCorners; }
            set
            {
                if (value != _compensateCorners)
                {
                    _compensateCorners = value;
                    Rebuild();
                }
            }
        }

        public float shapeExposure
        {
            get { return _shapeExposure; }
            set
            {
                if (spline != null && value != _shapeExposure)
                {
                    _shapeExposure = value;
                    Rebuild();
                }
            }
        }

        public AnimationCurve shape
        {
            get { return _shape; }
            set
            {
                if(_lastShape == null) _lastShape = new AnimationCurve();
                bool keyChange = false;
                if (value.keys.Length != _lastShape.keys.Length) keyChange = true;
                else
                {
                    for (int i = 0; i < value.keys.Length; i++)
                    {
                        if (value.keys[i].inTangent != _lastShape.keys[i].inTangent || value.keys[i].outTangent != _lastShape.keys[i].outTangent || value.keys[i].time != _lastShape.keys[i].time || value.keys[i].value != value.keys[i].value)
                        {
                            keyChange = true;
                            break;
                        }
                    }
                }
                if (keyChange) Rebuild();
                _lastShape.keys = new Keyframe[value.keys.Length];
                value.keys.CopyTo(_lastShape.keys, 0);
                _lastShape.preWrapMode = value.preWrapMode;
                _lastShape.postWrapMode = value.postWrapMode;
                _shape = value;

            }
        }

        protected override string meshName => "Path";

        [SerializeField]
        [HideInInspector]
        private int _slices = 1;
        [SerializeField]
        [HideInInspector]
        [Tooltip("This will inflate sample sizes based on the angle between two samples in order to preserve geometry width")]
        private bool _compensateCorners = false;
        [SerializeField]
        [HideInInspector]
        private bool _useShapeCurve = false;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve _shape;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve _lastShape;
        [SerializeField]
        [HideInInspector]
        private float _shapeExposure = 1f;


        protected override void Reset()
        {
            base.Reset();
        }


        /*protected override void BuildMesh()
        {
           base.BuildMesh();
           GenerateVertices();
           MeshUtility.GeneratePlaneTriangles(ref _tsMesh.triangles, _slices, sampleCount, false);
        }*/
        protected override void BuildMesh()
        {
            base.BuildMesh();
            GenerateThickMesh();
        }


        void GenerateVertices()
        {
            int vertexCount = (_slices + 1) * sampleCount;
            AllocateMesh(vertexCount, _slices * (sampleCount-1) * 6);
            int vertexIndex = 0;

            ResetUVDistance();

            bool hasOffset = offset != Vector3.zero;
            for (int i = 0; i < sampleCount; i++)
            {
                if (_compensateCorners)
                {
                    GetSampleWithAngleCompensation(i, ref evalResult);
                }
                else
                {
                    GetSample(i, ref evalResult);
                }

                Vector3 center = Vector3.zero;
                try
                {
                   center = evalResult.position;
                } catch (System.Exception ex) { Debug.Log(ex.Message + " for i = " + i); return; }
                Vector3 right = evalResult.right;
                float resultSize = GetBaseSize(evalResult);
                if (hasOffset)
                {
                    center += (offset.x * resultSize) * right + (offset.y * resultSize) * evalResult.up + (offset.z * resultSize) * evalResult.forward;
                }
                float fullSize = size * resultSize;
                Vector3 lastVertPos = Vector3.zero;
                Quaternion rot = Quaternion.AngleAxis(rotation, evalResult.forward);
                if (uvMode == UVMode.UniformClamp || uvMode == UVMode.UniformClip) AddUVDistance(i);
                Color vertexColor = GetBaseColor(evalResult) * color;
                for (int n = 0; n < _slices + 1; n++)
                {
                    float slicePercent = ((float)n / _slices);
                    float shapeEval = 0f;
                    if (_useShapeCurve) shapeEval = _shape.Evaluate(slicePercent);
                    _tsMesh.vertices[vertexIndex] = center + rot * right * (fullSize * 0.5f) - rot * right * (fullSize * slicePercent) + rot * evalResult.up * (shapeEval * _shapeExposure);
                    CalculateUVs(evalResult.percent, 1f - slicePercent);
                    _tsMesh.uv[vertexIndex] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - __uvs));
                    if (_slices > 1)
                    {
                        if (n < _slices)
                        {
                            float forwardPercent = ((float)(n + 1) / _slices);
                            shapeEval = 0f;
                            if (_useShapeCurve) shapeEval = _shape.Evaluate(forwardPercent);
                            Vector3 nextVertPos = center + rot * right * fullSize * 0.5f - rot * right * fullSize * forwardPercent + rot * evalResult.up * shapeEval * _shapeExposure;
                            Vector3 cross1 = -Vector3.Cross(evalResult.forward, nextVertPos - _tsMesh.vertices[vertexIndex]).normalized;

                            if (n > 0)
                            {
                                Vector3 cross2 = -Vector3.Cross(evalResult.forward, _tsMesh.vertices[vertexIndex] - lastVertPos).normalized;
                                _tsMesh.normals[vertexIndex] = Vector3.Slerp(cross1, cross2, 0.5f);
                            } else _tsMesh.normals[vertexIndex] = cross1;
                        }
                        else   _tsMesh.normals[vertexIndex] = -Vector3.Cross(evalResult.forward, _tsMesh.vertices[vertexIndex] - lastVertPos).normalized;
                    }
                    else
                    {
                        _tsMesh.normals[vertexIndex] = evalResult.up;
                        if (rotation != 0f) _tsMesh.normals[vertexIndex] = rot * _tsMesh.normals[vertexIndex];
                    }
                    _tsMesh.colors[vertexIndex] = vertexColor;
                    lastVertPos = _tsMesh.vertices[vertexIndex];
                    vertexIndex++;
                }
            }
        }
        void GenerateThickMesh()
        {
            int vertsPerRow = _slices + 1;
            int totalTopVerts = vertsPerRow * sampleCount;
            int totalVerts = totalTopVerts * 2; // top + bottom

            // Estimate triangle count:
            int topTris = _slices * (sampleCount - 1) * 6;
            int bottomTris = topTris;
            int sideTris = (sampleCount - 1) * 12; // 2 sides
            int totalTris = topTris + bottomTris + sideTris;

            AllocateMesh(totalVerts, totalTris);

            int vertexIndex = 0;

            Vector3[,] topVertices = new Vector3[sampleCount, vertsPerRow];
            Vector3[,] bottomVertices = new Vector3[sampleCount, vertsPerRow];

            // --- CREATE TOP + BOTTOM VERTICES ---
            for (int i = 0; i < sampleCount; i++)
            {
                GetSample(i, ref evalResult);

                Vector3 center = evalResult.position;
                Vector3 right = evalResult.right;
                Vector3 up = evalResult.up;

                float fullSize = size * GetBaseSize(evalResult);
                Quaternion rot = Quaternion.AngleAxis(rotation, evalResult.forward);

                for (int n = 0; n < vertsPerRow; n++)
                {
                    float percent = (float)n / _slices;

                    Vector3 top =
                        center +
                        rot * right * (fullSize * 0.5f) -
                        rot * right * (fullSize * percent);

                    Vector3 bottom = top - up * _thickness;

                    topVertices[i, n] = top;
                    bottomVertices[i, n] = bottom;

                    // TOP
                    _tsMesh.vertices[vertexIndex] = top;
                    _tsMesh.normals[vertexIndex] = up;
                    _tsMesh.uv[vertexIndex] = new Vector2(percent, (float)i / sampleCount);
                    vertexIndex++;

                    // BOTTOM
                    _tsMesh.vertices[vertexIndex] = bottom;
                    _tsMesh.normals[vertexIndex] = -up;
                    _tsMesh.uv[vertexIndex] = new Vector2(percent, (float)i / sampleCount);
                    vertexIndex++;
                }
            }

            // --- CREATE TRIANGLES ---
            int triIndex = 0;

            for (int i = 0; i < sampleCount - 1; i++)
            {
                for (int n = 0; n < _slices; n++)
                {
                    int topA = (i * vertsPerRow + n) * 2;
                    int topB = ((i + 1) * vertsPerRow + n) * 2;
                    int topC = topA + 2;
                    int topD = topB + 2;

                    // TOP
                    _tsMesh.triangles[triIndex++] = topA;
                    _tsMesh.triangles[triIndex++] = topB;
                    _tsMesh.triangles[triIndex++] = topC;

                    _tsMesh.triangles[triIndex++] = topC;
                    _tsMesh.triangles[triIndex++] = topB;
                    _tsMesh.triangles[triIndex++] = topD;

                    // BOTTOM (reverse order)
                    _tsMesh.triangles[triIndex++] = topA + 1;
                    _tsMesh.triangles[triIndex++] = topC + 1;
                    _tsMesh.triangles[triIndex++] = topB + 1;

                    _tsMesh.triangles[triIndex++] = topC + 1;
                    _tsMesh.triangles[triIndex++] = topD + 1;
                    _tsMesh.triangles[triIndex++] = topB + 1;
                }

                // SIDE WALLS (left + right edges)

                int leftTopA = (i * vertsPerRow) * 2;
                int leftTopB = ((i + 1) * vertsPerRow) * 2;

                int rightTopA = (i * vertsPerRow + _slices) * 2;
                int rightTopB = ((i + 1) * vertsPerRow + _slices) * 2;

                // LEFT
                _tsMesh.triangles[triIndex++] = leftTopA;
                _tsMesh.triangles[triIndex++] = leftTopA + 1;
                _tsMesh.triangles[triIndex++] = leftTopB;

                _tsMesh.triangles[triIndex++] = leftTopB;
                _tsMesh.triangles[triIndex++] = leftTopA + 1;
                _tsMesh.triangles[triIndex++] = leftTopB + 1;

                // RIGHT
                _tsMesh.triangles[triIndex++] = rightTopA;
                _tsMesh.triangles[triIndex++] = rightTopB;
                _tsMesh.triangles[triIndex++] = rightTopA + 1;

                _tsMesh.triangles[triIndex++] = rightTopB;
                _tsMesh.triangles[triIndex++] = rightTopB + 1;
                _tsMesh.triangles[triIndex++] = rightTopA + 1;
            }
        }
    }
}
