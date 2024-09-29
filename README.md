# Who-Could-Have-You-Been
Who would you be if you were born in a random country between 1960 and 2023?

This Unity project is a visualization tool that displays country populations over different years using a rotating world map UI. The player can select a year using a slider, view the global population for the chosen year, and simulate what it would be like to be born in a random country using the "WhoCouldHaveYouBeen" button. The application uses XML files to load data about countries and their respective populations, and dynamically updates the UI to display relevant information.

How It Works:


Load XML Files:

The SetXmlDocument method reads and loads XML data for country populations and world data using Resources.Load<TextAsset>().

Query Data:

XPath queries like SelectSingleNode and SelectNodes are used to retrieve specific records for countries or years.

Random Country Selection:

The app randomly selects a country based on cumulative population values, using a custom NextLong() method to generate a random index.

UI Updates:

Text elements display selected year and country data, while tooltips and popups show the country and its population for the chosen year.

Extensions:

Utility functions format large numbers (1,000,000 -> 1M) and help generate random long values.

/-*-\

Data taken from: (https://data.worldbank.org/indicator/SP.POP.TOTL?end=2023&start=1960)
