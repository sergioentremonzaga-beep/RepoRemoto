Se ha optado por el uso de records para el modelo user porque no hay necesidad de que sea mutable, lo mismo para los dto relacionados con la api, son datos que nunca van a mutar. En cambio para la entity del usuario, como EFCore requiere que sea mutable se optó por una clase.  
La estructura de datos en 3 niveles permite una mayor eficiencia gracias a la cache, para datos que se utilizan comúnmente se leerán más rápido. Los datos almacenados en el repositorio local permiten que en caso de que la API no funcione siga habiendo opción para consultarlos.
y la API será la fuente de datos principal.  
Los errores se manejan con Result<T, Error> porque son menos costosos que las excepciones y la API los mapea automáticamente a su código HTTP.
