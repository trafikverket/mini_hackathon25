import requests
import config

API_URL = 'https://api.trafikinfo.trafikverket.se/v2/data.json'

def fetch_parkings(limit: int):
    xml_request = f"""
        <REQUEST>
            <LOGIN authenticationkey="{config.TRAFIKVERKET_API_KEY}"/>
            <QUERY objecttype="Parking" namespace="road.infrastructure" schemaversion="1.4" limit="{limit}">
                <FILTER></FILTER>
            </QUERY>
        </REQUEST>
    """

    try:
        response = requests.post(API_URL, headers={'Content-Type': 'text/xml'}, data=xml_request.strip().encode('UTF-8'))
        response.raise_for_status()

        return response.json()
    except requests.exceptions.RequestException as ex:
        return None
    except Exception as ex:
        return None
