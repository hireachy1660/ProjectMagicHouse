using UnityEngine;
using VRPortalToolkit.Utilities;

namespace VRPortalToolkit.Data
{
    public struct ViewWindow
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
        public float zMin;
        public float zMax;

        public ViewWindow(float width, float height, float depth)
        {
            this.xMin = 0f;
            this.xMax = width;
            this.yMin = 0f;
            this.yMax = height;
            this.zMin = depth;
            this.zMax = depth;
        }

        public ViewWindow(float min, float max)
        {
            this.xMin = min;
            this.xMax = max;
            this.yMin = min;
            this.yMax = max;
            this.zMin = min;
            this.zMax = max;
        }

        public ViewWindow(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.zMin = zMin;
            this.zMax = zMax;
        }

        public bool IsValid() => xMin <= xMax && yMin <= yMax && zMin <= zMax;

        public bool Contains(Vector2 screenPos)
            => xMin <= screenPos.x && screenPos.x <= xMax && yMin <= screenPos.y && screenPos.y <= yMax;

        public bool Contains(Vector3 screenPos)
            => xMin <= screenPos.x && screenPos.x <= xMax && yMin <= screenPos.y && screenPos.y <= yMax && zMin <= screenPos.z && screenPos.z <= zMax;

        public void AddPoint(Vector3 point)
        {
            xMin = Mathf.Min(xMin, point.x);
            xMax = Mathf.Max(xMax, point.x);
            yMin = Mathf.Min(yMin, point.y);
            yMax = Mathf.Max(yMax, point.y);
            zMin = Mathf.Min(zMin, point.z);
            zMax = Mathf.Max(zMax, point.z);
        }

        // [수정 포인트 1] 가시성 판단 로그
        public bool IsVisibleThrough(ViewWindow outerWindow)
        {
            bool visible = (zMax >= outerWindow.zMin && xMax > outerWindow.xMin && xMin < outerWindow.xMax && yMax > outerWindow.yMin && yMin < outerWindow.yMax);

            // 만약 포탈이 그려져야 하는데 안 그려진다면 아래 로그를 활성화하세요.
            // if (!visible) Debug.Log($"[Culling] Portal is hidden. Outer: {outerWindow}, Inner: {this}");

            return visible;
        }

        public void ClampInside(ViewWindow outerWindow)
        {
            if (xMin < outerWindow.xMin) xMin = outerWindow.xMin;
            if (xMax > outerWindow.xMax) xMax = outerWindow.xMax;
            if (yMin < outerWindow.yMin) yMin = outerWindow.yMin;
            if (yMax > outerWindow.yMax) yMax = outerWindow.yMax;
        }

        private static readonly Vector3[] boundCornerOffsets = {
            new Vector3 (1, 1, 1), new Vector3 (-1, 1, 1), new Vector3 (-1, -1, 1), new Vector3 (-1, -1, -1),
            new Vector3 (-1, 1, -1), new Vector3 (1, -1, -1), new Vector3 (1, 1, -1), new Vector3 (1, -1, 1),
        };

        public Rect GetRect() => Rect.MinMaxRect(xMin, yMin, xMax, yMax);

        public static ViewWindow Combine(ViewWindow windowA, ViewWindow windowB)
        {
            if (windowA.IsValid())
            {
                if (windowB.IsValid())
                {
                    if (windowB.xMin < windowA.xMin) windowA.xMin = windowB.xMin;
                    if (windowB.yMin < windowA.yMin) windowA.yMin = windowB.yMin;
                    if (windowB.zMin < windowA.zMin) windowA.zMin = windowB.zMin;
                    if (windowB.xMax > windowA.xMax) windowA.xMax = windowB.xMax;
                    if (windowB.yMax > windowA.yMax) windowA.yMax = windowB.yMax;
                    if (windowB.zMax > windowA.zMax) windowA.zMax = windowB.zMax;
                }
                return windowA;
            }
            return windowB;
        }

        // [수정 포인트 2] 윈도우 계산 로그 추가
        public static ViewWindow GetWindow(Matrix4x4 view, Matrix4x4 proj, Bounds localBounds, Matrix4x4 localToWorld)
        {
            ViewWindow window = new ViewWindow(float.MaxValue, float.MinValue);
            Vector3 corner;

            for (int i = 0; i < 8; i++)
            {
                corner = localBounds.center + Vector3.Scale(localBounds.extents, boundCornerOffsets[i]);
                corner = localToWorld.MultiplyPoint(corner);
                corner = CameraUtility.WorldToViewportPoint(view, proj, corner);

                if (corner.z <= 0f)
                {
                    window.xMin = -1f; // 뒤에 있을 때 판정을 넉넉하게 수정
                    window.xMax = 2f;
                    window.yMin = -1f;
                    window.yMax = 2f;
                    window.zMin = Mathf.Min(window.zMin, corner.z);
                    window.zMax = Mathf.Max(window.zMax, corner.z);
                }

                window.AddPoint(corner);
            }

            // [디버그 로그] 계산된 포탈의 화면 좌표 출력
            // x, y 값이 0~1 사이여야 화면 안에 있는 것입니다.
            // Debug.Log($"[ViewWindow] Calculated Rect: x({window.xMin:F2}~{window.xMax:F2}), y({window.yMin:F2}~{window.yMax:F2})");

            return window;
        }

        public static ViewWindow GetWindow(Camera camera, Bounds localBounds, Matrix4x4 localToWorld)
        {
            ViewWindow window = new ViewWindow(float.MaxValue, float.MinValue);
            Vector3 corner;
            for (int i = 0; i < 8; i++)
            {
                corner = localBounds.center + Vector3.Scale(localBounds.extents, boundCornerOffsets[i]);
                corner = localToWorld.MultiplyPoint(corner);
                corner = camera.WorldToViewportPoint(corner);
                if (corner.z <= 0)
                {
                    window.xMin = -1f;
                    window.xMax = 2f;
                    window.yMin = -1f;
                    window.yMax = 2f;
                    window.zMin = Mathf.Min(window.zMin, corner.z);
                    window.zMax = Mathf.Max(window.zMax, corner.z);
                }
                window.AddPoint(corner);
            }
            return window;
        }

        public override string ToString() => $"({xMin:F2}<{xMax:F2}, {yMin:F2}<{yMax:F2}, {zMin:F2}<{zMax:F2})";
    }
}