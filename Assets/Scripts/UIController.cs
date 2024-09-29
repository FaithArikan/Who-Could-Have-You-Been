using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace WhoYouCouldHaveBeen
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance;

        #region Data

        public int currentYear;
        private string _selectedCountry;
        private long _selectedCountryPopulation;

        #endregion

        #region XML

        private XmlDocument _xmlCountryDocument;
        private XmlDocument _xmlWorldDocument;

        #endregion

        #region Slider

        public Slider slider;
        public TextMeshProUGUI sliderValueText;

        #endregion

        #region ToolTip

        public GameObject toolTip;
        public TextMeshProUGUI toolTipText;
        public TextMeshProUGUI worldPopText;

        #endregion

        #region WorldWheel

        [SerializeField] private List<Renderer> worldMapWheelCountryRenderers;
        [SerializeField] private Transform mapWheelPanel;
        [SerializeField] private int wheelCount = 10;
        private List<int> _unusedIndices;
        public Material countryMaterial;
        public Material mouseEnterMaterial;

        #endregion

        #region Popup

        [SerializeField] private GameObject popup;
        [SerializeField] private TextMeshProUGUI popupText;

        #endregion

        private void Awake()
        {
            Instance = this;

            _xmlCountryDocument = SetXmlDocument("modified_country_data");
            _xmlWorldDocument = SetXmlDocument("WorldData");

            UpdateText(slider.value);

            slider.onValueChanged.AddListener(UpdateText);

            InitializeUnusedIndices();

            mapWheelPanel.gameObject.SetActive(false);
            popup.SetActive(false);
        }

        private void UpdateText(float value)
        {
            sliderValueText.text = value.ToString("F0");

            currentYear = (int)value;

            worldPopText.text = "  World Population: " +
                                Extensions.FormatBigNumber(GetValueForCountryAndYear("", currentYear.ToString(),
                                    false));
        }
        
        public void WhoYouCouldHaveBeenButtonClick()
        {
            (string Country, long Value) result = GetRandomCountryAndValueByYear(currentYear);

            StartCoroutine(ChangeRandomChildMaterial(result.Country, result.Value));
        }

        private (string Country, long Value) GetRandomCountryAndValueByYear(int targetYear)
        {
            XmlNodeList recordNodes =
                _xmlCountryDocument.SelectNodes($"//record[field[@name='Year'] = '{targetYear}']");

            if (recordNodes is { Count: 0 })
            {
                throw new Exception($"No records found for the year {targetYear}.");
            }

            var cumulativeValues = new List<long>();
            var countryValuePairs = new List<(string Country, long Value)>();
            long totalValue = 0;

            if (recordNodes != null)
            {
                foreach (XmlNode record in recordNodes)
                {
                    string country = record.SelectSingleNode("field[@name='Country']")?.InnerText;
                    string valueStr = record.SelectSingleNode("field[@name='Value']")?.InnerText;

                    if (country != null && valueStr != null && long.TryParse(valueStr, out long value))
                    {
                        totalValue += value;
                        cumulativeValues.Add(totalValue);
                        countryValuePairs.Add((country, value));
                    }
                }
            }

            if (cumulativeValues.Count == 0)
            {
                throw new Exception($"No valid data found for the year {targetYear}.");
            }

            System.Random rand = new();
            long randomValue = rand.NextLong(1, totalValue);

            for (int i = 0; i < cumulativeValues.Count; i++)
            {
                if (randomValue <= cumulativeValues[i])
                {
                    return countryValuePairs[i];
                }
            }

            throw new Exception("Unable to find a valid country and value for the given year.");
        }

        public long GetValueForCountryAndYear(string country, string year, bool isCountry = true)
        {
            XmlNode recordNode;

            if (isCountry)
            {
                recordNode =
                    _xmlCountryDocument.SelectSingleNode(
                        $"//record[field[@name='Country'] = '{country}' and field[@name='Year'] = '{year}']");
            }
            else
            {
                recordNode = _xmlWorldDocument.SelectSingleNode($"//record[field[@name='Year'] = '{year}']");
            }

            if (recordNode != null)
            {
                if (long.TryParse(recordNode.LastChild.InnerText, out long value))
                {
                    return value;
                }
            }
            else
            {
                return -1;
            }

            return -1;
        }
        
        private IEnumerator ChangeRandomChildMaterial(string country, long val)
        {
            mapWheelPanel.gameObject.SetActive(true);
            popup.SetActive(false);

            for (int i = 0; i < wheelCount; i++)
            {
                if (_unusedIndices.Count == 0)
                {
                    InitializeUnusedIndices();
                }

                int randomChildIndex = _unusedIndices[Random.Range(0, _unusedIndices.Count)];
                Renderer randomChildRenderer = worldMapWheelCountryRenderers[randomChildIndex];
                _unusedIndices.Remove(randomChildIndex);

                if (randomChildRenderer != null)
                {
                    randomChildRenderer.material = mouseEnterMaterial;

                    yield return new WaitForSeconds(0.05f);

                    randomChildRenderer.material = countryMaterial;
                }

                yield return new WaitForSeconds(0.1f);
            }

            _selectedCountry = country;
            _selectedCountryPopulation = val;

            mapWheelPanel.gameObject.SetActive(false);
            SetPopup(
                $"In <b>{currentYear}</b> you could have been born in <b>{_selectedCountry}</b> with <b>{Extensions.FormatBigNumber(_selectedCountryPopulation)}</b> inhabitants.");
        }

        private void InitializeUnusedIndices()
        {
            _unusedIndices = new List<int>();

            for (int i = 0; i < worldMapWheelCountryRenderers.Count; i++)
            {
                _unusedIndices.Add(i);
            }
        }

        private void SetPopup(string popupStr)
        {
            popup.SetActive(true);
            popupText.text = popupStr;
        }

        private static XmlDocument SetXmlDocument(string path)
        {
            TextAsset xmlTextAsset = Resources.Load<TextAsset>($"Data/{path}");
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlTextAsset.text);
            return xmlDoc;
        }

        public void OpenLink(string link)
        {
            Application.OpenURL(link);
        }
    }

    public static class Extensions
    {
        public static string FormatBigNumber(long number)
        {
            switch (number)
            {
                case >= 1_000_000_000:
                    return (number / 1_000_000_000.0).ToString("0.##") + "B";
                case >= 1_000_000:
                    return (number / 1_000_000.0).ToString("0.##") + "M";
                case >= 1_000:
                    return (number / 1_000.0).ToString("0.##") + "K";
                default:
                    return number.ToString();
            }
        }

        public static long NextLong(this System.Random rand, long min, long max)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }
    }
}