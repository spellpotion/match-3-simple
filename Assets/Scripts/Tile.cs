using System;
using System.Collections;
using UnityEngine;

namespace Match3Simple
{
    public class Tile : MonoBehaviour
    {
        public static readonly float durationAnimation = .5f;

        public static Action<Tile> OnTileSelected;

        public bool IsMatch { get; set; } = false;

        // move-to position
        private Vector3 localPosition;
        // animation
        private Coroutine moveToPosition;

        private void Start()
        {
            localPosition = transform.localPosition;
        }

        private void OnMouseDown()
        {
            if (moveToPosition == null)
            {
                OnTileSelected?.Invoke(this);
            }
        }

        public bool SetPosition(Vector3 localPosition)
        {
            if (this.localPosition == localPosition) return false;

            if (moveToPosition != null)
            {
                StopCoroutine(moveToPosition);
                CleanUp();
            }

            this.localPosition = localPosition;
            moveToPosition = StartCoroutine(MoveToPosition());

            return true;
        }

        private IEnumerator MoveToPosition()
        {
            var localPositionStart = transform.localPosition;
            var localPositionDifference = localPosition - localPositionStart;

            var timeStart = Time.time;
            var progress = 0f;
            while (progress < 1f)
            {
                progress = (Time.time - timeStart) / durationAnimation;
                var smooth = Mathf.SmoothStep(0f, 1f, progress);

                transform.localPosition = localPositionStart + localPositionDifference * smooth;

                yield return new WaitForEndOfFrame();
            }

            CleanUp();
        }

        private void CleanUp()
        {
            transform.localPosition = localPosition;
            moveToPosition = null;
        }
    }
}