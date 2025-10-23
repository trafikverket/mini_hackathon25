# TrafikverketHackaton2025

- William Mossberg h24wilmo@du.se
- Viktor Eriksson h24verik@du.se
- Andreas Lindström h24andrl@du.se

Ett program som visar alla trafikverkets parkeringar med information om lastbilsparkeringar.
Programmet kan rita ut rutter till parkeringar och städer. Programmet visar även körtid till parkeringarna och städerna. När en rutt vissas visar kartan enbart parkeringar längs med rutten.

# Kör projektet
```sh
touch .env
echo "TRAFIKVERKET_API_KEY=din-api-nyckel" >> .env
python3 -m venv .venv
source .venv/bin/activate
pip3 install -r requirements.txt
python3 app.py
```
