from flask import Flask, render_template, jsonify, send_from_directory, request
from parkings import fetch_parkings
import requests

app = Flask(__name__)

@app.route('/')
def view_map():
    return render_template('map.html')

@app.route('/api/parkings', methods=['GET'])
def get_parkings():
    return jsonify(fetch_parkings(10000))

@app.route('/api/geocode', methods=['GET'])
def geocode():
    query = request.args.get('q', '')
    if not query:
        return jsonify({'error': 'No search query provided'}), 400

    # Använd Nominatim (OpenStreetMap's geocoding service)
    url = 'https://nominatim.openstreetmap.org/search'
    params = {
        'q': query,
        'format': 'json',
        'limit': 1,
        'countrycodes': 'se'  # Begränsa till Sverige
    }
    headers = {
        'User-Agent': 'TrafikverketHackaton2025'
    }

    try:
        response = requests.get(url, params=params, headers=headers)
        response.raise_for_status()
        data = response.json()

        if data:
            return jsonify({
                'lat': float(data[0]['lat']),
                'lon': float(data[0]['lon']),
                'display_name': data[0]['display_name']
            })
        else:
            return jsonify({'error': 'Location not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run()



