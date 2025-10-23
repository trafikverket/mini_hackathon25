import os
from dotenv import load_dotenv

load_dotenv()

TRAFIKVERKET_API_KEY = os.getenv('TRAFIKVERKET_API_KEY')
OPEN_ROUTE_SERVICE_API_KEY = os.getenv('OPEN_ROUTE_SERVICE_API_KEY')
