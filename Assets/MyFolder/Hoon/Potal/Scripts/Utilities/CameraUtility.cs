using UnityEngine;
using UnityEngine.Rendering;

namespace VRPortalToolkit.Utilities
{
    /// <summary>
    /// 포탈 렌더링 관련 카메라 연산을 담당하는 유틸리티 클래스입니다.
    /// PlaneIntersection이 추가된 최종 버전입니다.
    /// </summary>
    public static class CameraUtility
    {
        #region 가시성 체크 (Visibility)
        public static bool VisibleFromCamera(this Renderer renderer, Camera camera)
            => GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);

        public static bool VisibleFromCameraPlanes(this Renderer renderer, Plane[] planes)
            => GeometryUtility.TestPlanesAABB(planes, renderer.bounds);

        public static bool VisibleFromCamera(this Bounds bounds, Camera camera)
            => GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), bounds);

        public static bool VisibleFromCameraPlanes(this Bounds bounds, Plane[] planes)
            => GeometryUtility.TestPlanesAABB(planes, bounds);
        #endregion

        #region 평면 교차 연산 (Plane Intersection)
        /// <summary>
        /// 두 평면이 교차하는 지점(position)과 방향(direction)을 계산합니다.
        /// PortalCameraTransitionRenderer에서 사용됩니다.
        /// </summary>
        public static bool PlaneIntersection(Plane plane1, Plane plane2, out Vector3 position, out Vector3 direction)
        {
            direction = Vector3.Cross(plane1.normal, plane2.normal);
            float denominator = Vector3.Dot(direction, direction);

            if (denominator > 0.000001f)
            {
                Vector3 temp = Vector3.Cross((-plane1.distance * plane2.normal) + (plane2.distance * plane1.normal), direction);
                position = temp / denominator;
                return true;
            }

            position = Vector3.zero;
            direction = Vector3.zero;
            return false;
        }
        #endregion

        #region 스테레오/VR (Stereo)
        public static Vector3 GetStereoOffset(this Camera camera, Camera.MonoOrStereoscopicEye eye)
        {
            Vector3 offset = camera.transform.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f), eye));
            if (eye == Camera.MonoOrStereoscopicEye.Right) { if (offset.x < 0) offset.x = -offset.x; }
            else if (offset.x > 0) offset.x = -offset.x;
            return offset;
        }

        public static Matrix4x4 GetStereoProjectionMatrixFixed(this Camera camera, Camera.StereoscopicEye eye)
        {
            Matrix4x4 projectionMatrix = camera.GetStereoProjectionMatrix(eye);
            if (eye == Camera.StereoscopicEye.Right) { if (projectionMatrix.m02 < 0) projectionMatrix.m02 = -projectionMatrix.m02; }
            else if (projectionMatrix.m02 > 0) projectionMatrix.m02 = -projectionMatrix.m02;
            return projectionMatrix;
        }

        public static void GetStereoCamera(this Camera camera, Camera.MonoOrStereoscopicEye eye, out Vector3 offset, out Matrix4x4 projectionMatrix)
        {
            switch (eye)
            {
                case Camera.MonoOrStereoscopicEye.Left:
                    offset = camera.GetStereoOffset(eye);
                    projectionMatrix = camera.GetStereoProjectionMatrixFixed(Camera.StereoscopicEye.Left);
                    break;
                case Camera.MonoOrStereoscopicEye.Right:
                    offset = camera.GetStereoOffset(eye);
                    projectionMatrix = camera.GetStereoProjectionMatrixFixed(Camera.StereoscopicEye.Right);
                    break;
                default:
                    offset = Vector3.zero;
                    projectionMatrix = camera.projectionMatrix;
                    break;
            }
        }
        #endregion

        #region 사선 투영 (Oblique Projection)
        public static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 view, Matrix4x4 proj, Vector3 clippingPlaneCentre, Vector3 clippingPlaneNormal)
        {
            Plane clippingPlane = new Plane(-clippingPlaneNormal, clippingPlaneCentre);
            Vector4 clipPlane = new Vector4(clippingPlane.normal.x, clippingPlane.normal.y, clippingPlane.normal.z, clippingPlane.distance);
            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(view)) * clipPlane;

            Matrix4x4 obliqueProj = CalculateObliqueMatrix(proj, clipPlaneCameraSpace);
            return (obliqueProj[14] <= -0.001f) ? obliqueProj : proj;
        }

        private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4(Mathf.Sign(clipPlane.x), Mathf.Sign(clipPlane.y), 1.0f, 1.0f);
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];
            return projection;
        }
        #endregion

        #region 좌표 변환 (Coordinate Conversion)
        public static Vector3 WorldToViewportPoint(in Matrix4x4 view, in Matrix4x4 proj, Vector3 point)
        {
            Matrix4x4 VP = proj * view;
            Vector4 result4 = VP * new Vector4(point.x, point.y, point.z, 1.0f);
            if (result4.w == 0) return new Vector3(0.5f, 0.5f, 0f);
            Vector3 result = (Vector3)result4 / -result4.w;
            result.x = -result.x * 0.5f + 0.5f;
            result.y = -result.y * 0.5f + 0.5f;
            result.z = result4.w;
            return result;
        }

        public static Vector3 ViewportToWorldPoint(Matrix4x4 view, Matrix4x4 proj, Vector3 point)
        {
            Vector4 clipPos = new Vector4(point.x * 2 - 1, point.y * 2 - 1, point.z, 1f);
            Vector4 worldPos = proj.inverse * clipPos;
            return view.inverse.MultiplyPoint(worldPos);
        }
        #endregion

        #region 가위 행렬 (Scissor Matrix)
        public static Matrix4x4 CalculateScissorMatrix(this Matrix4x4 proj, Rect rect)
        {
            if (rect.width <= 0 || rect.height <= 0) return proj;
            rect.x = Mathf.Max(0, rect.x); rect.y = Mathf.Max(0, rect.y);
            rect.width = Mathf.Min(1 - rect.x, rect.width); rect.height = Mathf.Min(1 - rect.y, rect.height);

            Matrix4x4 m1 = Matrix4x4.TRS(new Vector3((1 / rect.width - 1), (1 / rect.height - 1), 0), Quaternion.identity, new Vector3(1 / rect.width, 1 / rect.height, 1)),
                      m2 = Matrix4x4.TRS(new Vector3(-rect.x * 2 / rect.width, -rect.y * 2 / rect.height, 0), Quaternion.identity, Vector3.one);
            return m2 * m1 * proj;
        }
        #endregion

        #region 커맨드 버퍼 및 스테레오 상수
        private static StereoConstants _stereoConstants;
        private static readonly Vector4[] _stereoEyeIndices = { Vector4.zero, Vector4.one };

        public static void SetStereoViewProjectionMatrices(this CommandBuffer commandBuffer, Matrix4x4 leftView, Matrix4x4 leftProj, Matrix4x4 rightView, Matrix4x4 rightProj)
        {
            if (_stereoConstants == null) _stereoConstants = new StereoConstants();

            _stereoConstants.viewMatrix[0] = leftView;
            _stereoConstants.projMatrix[0] = leftProj;
            _stereoConstants.viewMatrix[1] = rightView;
            _stereoConstants.projMatrix[1] = rightProj;

            for (int i = 0; i < 2; i++)
            {
                _stereoConstants.gpuProjectionMatrix[i] = GL.GetGPUProjectionMatrix(_stereoConstants.projMatrix[i], true);
                _stereoConstants.viewProjMatrix[i] = _stereoConstants.gpuProjectionMatrix[i] * _stereoConstants.viewMatrix[i];
                _stereoConstants.invViewMatrix[i] = Matrix4x4.Inverse(_stereoConstants.viewMatrix[i]);
                _stereoConstants.invGpuProjMatrix[i] = Matrix4x4.Inverse(_stereoConstants.gpuProjectionMatrix[i]);
                _stereoConstants.invViewProjMatrix[i] = Matrix4x4.Inverse(_stereoConstants.viewProjMatrix[i]);
                _stereoConstants.invProjMatrix[i] = Matrix4x4.Inverse(_stereoConstants.projMatrix[i]);
                _stereoConstants.worldSpaceCameraPos[i] = _stereoConstants.invViewMatrix[i].GetColumn(3);
            }

            commandBuffer.SetGlobalMatrixArray("unity_StereoMatrixV", _stereoConstants.viewMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoMatrixP", _stereoConstants.gpuProjectionMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoMatrixVP", _stereoConstants.viewProjMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoCameraProjection", _stereoConstants.projMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoMatrixInvV", _stereoConstants.invViewMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoMatrixInvP", _stereoConstants.invGpuProjMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoMatrixInvVP", _stereoConstants.invViewProjMatrix);
            commandBuffer.SetGlobalMatrixArray("unity_StereoCameraInvProjection", _stereoConstants.invProjMatrix);
            commandBuffer.SetGlobalVectorArray("unity_StereoWorldSpaceCameraPos", _stereoConstants.worldSpaceCameraPos);
        }

        public static void StartSinglePass(CommandBuffer cmd)
        {
            if (SystemInfo.supportsMultiview)
            {
                cmd.EnableShaderKeyword("STEREO_MULTIVIEW_ON");
                cmd.SetGlobalVectorArray("unity_StereoEyeIndices", _stereoEyeIndices);
            }
            else
            {
                cmd.EnableShaderKeyword("STEREO_INSTANCING_ON");
                cmd.SetInstanceMultiplier(2);
            }
        }

        public static void StopSinglePass(CommandBuffer cmd)
        {
            cmd.DisableShaderKeyword("STEREO_MULTIVIEW_ON");
            cmd.DisableShaderKeyword("STEREO_INSTANCING_ON");
            cmd.SetInstanceMultiplier(1);
        }

        private class StereoConstants
        {
            public readonly Matrix4x4[] viewMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] gpuProjectionMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] projMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] viewProjMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] invViewMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] invGpuProjMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] invViewProjMatrix = new Matrix4x4[2];
            public readonly Matrix4x4[] invProjMatrix = new Matrix4x4[2];
            public readonly Vector4[] worldSpaceCameraPos = new Vector4[2];
        }
        #endregion
    }
}