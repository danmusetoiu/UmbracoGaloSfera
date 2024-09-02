 <script type="text/javascript">
     window.onload = function () {
         //var markers = eval('@Html.Raw(ViewData["Markers"])');
         // Initializing the Map.
         //var map = L.map('map').setView([markers[0].Lat, markers[0].Lng], 8);
         var map = L.map('dvMap').setView([44.505, 26.09], 13);

         // Setting the Attribution.
         L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
             maxZoom: 19,
             attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
         }).addTo(map);

         // Adding Marker to map.
         markers.forEach(function (item) {
             L.marker([item.Lat, item.Lng])
                 .bindPopup("<b>" + item.Title + "</b><br />" + item.Description).addTo(map);
         }
         );
     };
 </script>

                        