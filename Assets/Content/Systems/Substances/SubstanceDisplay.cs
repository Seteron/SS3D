using Mirror;
using SS3D.Engine.Substances;
using UnityEngine;

namespace SS3D.Content.Systems.Substances
{
    public class SubstanceDisplay : NetworkBehaviour
    {
        /// <summary>
        /// The container to display
        /// </summary>
        public SubstanceContainer Container;
        /// <summary>
        /// The object displaying the fluid level
        /// </summary>
        public GameObject DisplayObject;
        /// <summary>
        /// The position of fill when empty
        /// </summary>
        public Vector3 EmptyPosition;
        /// <summary>
        /// The position of fill when full
        /// </summary>
        public Vector3 FullPosition;
        public AnimationCurve ScaleX;
        public AnimationCurve ScaleY;
        public AnimationCurve ScaleZ;
        private MeshRenderer meshRenderer;

        // wobble shader stuff
        Vector3 lastPos;
        Vector3 velocity;
        Vector3 lastRot;
        Vector3 angularVelocity;
        public float MinFill = 0.01f;
        public float MaxFill = 0.99f;
        public float MaxWobble = 0.03f;
        public float WobbleSpeed = 1f;
        public float Recovery = 1f;
        float wobbleAmountX;
        float wobbleAmountZ;
        float wobbleAmountToAddX;
        float wobbleAmountToAddZ;
        float pulse;
        float time = 0.5f;
        float fillAmount = 0;

        private void Start()
        {
            meshRenderer = DisplayObject.GetComponent<MeshRenderer>();
            fillAmount = 0;
            if (isServer)
            {
                Container.ContentsChanged += container => UpdateDisplay();
                UpdateDisplay();
            }
        }

        private void Update()
        {
            time += Time.deltaTime;
            // decrease wobble over time
            wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * (Recovery));
            wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * (Recovery));

            // make a sine wave of the decreasing wobble
            pulse = 2 * Mathf.PI * WobbleSpeed;
            wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
            wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

            // send it to the shader
            meshRenderer.material.SetFloat("_WobbleX", wobbleAmountX);
            meshRenderer.material.SetFloat("_WobbleZ", wobbleAmountZ);
            Mesh mesh = DisplayObject.GetComponent<MeshFilter>().mesh;
            meshRenderer.material.SetFloat("_Height", meshRenderer.bounds.extents.y);
            float relativeVolume = (Container.CurrentVolume / Container.Volume);
            if(fillAmount != relativeVolume)
            {
                fillAmount = Mathf.Lerp(fillAmount, relativeVolume, Time.deltaTime * 1.5f);
                if(fillAmount < 0||float.IsNaN(fillAmount)) fillAmount = 0f;
                float clampedFill = fillAmount * (MaxFill - MinFill) + MinFill;
                meshRenderer.material.SetFloat("_FillAmount", fillAmount);
            }

            // velocity
            velocity = (lastPos - transform.position) / Time.deltaTime;
            angularVelocity = transform.rotation.eulerAngles - lastRot;


            // add clamped velocity to wobble
            wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
            wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

            // keep last position
            lastPos = transform.position;
            lastRot = transform.rotation.eulerAngles;
        }

        [Server]
        private void UpdateDisplay()
        {
            Transform trans = DisplayObject.transform;
            Color newColor = CalculateColor();
            Color.RGBToHSV(newColor , out var h, out var s, out var v);
            Color newColorTop = Color.HSVToRGB(h, s * 0.35f, v);
            Color newColorFoam = Color.HSVToRGB(h, s * 0.55f, v);
            meshRenderer.material.SetColor("_Tint", newColor);
            meshRenderer.material.SetColor("_TopColor", newColorTop);
            meshRenderer.material.SetColor("_FoamColor", newColorFoam);

            //trans.localPosition = Vector3.Lerp(EmptyPosition, FullPosition, Mathf.Min(relativeVolume, 1));
            //trans.localScale = new Vector3(ScaleX.Evaluate(relativeVolume), ScaleY.Evaluate(relativeVolume), ScaleZ.Evaluate(relativeVolume));
            RpcUpdateDisplay(trans.localPosition, trans.localScale, newColor);
        }

        private Color CalculateColor()
        {
            float totalMoles = Container.TotalMoles;
            Color color = new Color(0, 0, 0, 0);
            foreach (SubstanceEntry entry in Container.Substances)
            {
                float relativeMoles = entry.Moles / totalMoles;
                color += entry.Substance.Color * relativeMoles;
            }

            color.a = 1f;
            return color;
        }

        [ClientRpc]
        private void RpcUpdateDisplay(Vector3 position, Vector3 scale, Color color)
        {
            Transform trans = DisplayObject.transform;
            trans.localPosition = position;
            trans.localScale = scale;
            meshRenderer.material.color = color;
        }
    }
}
