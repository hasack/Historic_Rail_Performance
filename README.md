# RailAppWPF

The [National Rail website](https://www.nationalrail.co.uk/100296.aspx) has a suite of data feeds as part of its 'Darwin' service.

This app builds on the Historic Service Performance JSON API - details of which can be found [here](https://wiki.openraildata.com/index.php/HSP).

Set up an account through the website, input your details into the 'Preferences' menu whereupon they will be stored encrypted in AppData.

The app offers a list of train stations. Select To, From, a day and a time and, on the assumption there are direct train(s) between those destinations, it will query the API and pull back when the train left your 'From' location and arrived at the 'To', as well as including a comparison with the published times. 

MIT license - you're welcome to use it for free; but it's offered as is with no representations about it's fitness to be used (or liability if it turns out to have issues).
