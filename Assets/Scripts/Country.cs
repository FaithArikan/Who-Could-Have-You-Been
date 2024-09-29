using UnityEngine;

namespace WhoYouCouldHaveBeen
{
    public class Country : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private string countryName;

        private void Awake()
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnMouseEnter()
        {
            var uiController = UIController.Instance;
            
            meshRenderer.material = uiController.mouseEnterMaterial;
            
            uiController.toolTip.gameObject.SetActive(true);
            uiController.toolTip.transform.position = new Vector3(transform.position.x, 10, transform.position.z - 10);
            
            if (uiController.GetValueForCountryAndYear(countryName, uiController.currentYear.ToString()) == -1)
            {
                uiController.toolTipText.text = countryName + "\n" + "Data is missing";
            }
            else
            {
                uiController.toolTipText.text = countryName + "\n" + Extensions.FormatBigNumber(uiController.GetValueForCountryAndYear(countryName, uiController.currentYear.ToString()));
            }
        }

        private void OnMouseExit()
        {
            var uiController = UIController.Instance;

            meshRenderer.material = uiController.countryMaterial;

            if (uiController.toolTip != null)
            {
                uiController.toolTip.gameObject.SetActive(false);
            }
        }
    }
}