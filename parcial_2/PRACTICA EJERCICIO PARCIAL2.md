PRACTICA EJERCICIO PARCIAL 2

**1. Contexto del Problema**

Estás a cargo del desarrollo del módulo de *checkout* para una plataforma de comercio electrónico en Argentina. El núcleo del sistema de compras está diseñado para interactuar de forma genérica con cualquier sistema de cobros utilizando una única interfaz interna llamada IPasarelaPago.

El negocio necesita expandir sus opciones de pago integrando dos servicios externos muy solicitados:

1. **Mercado Pago** (el estándar local en pesos).

2. **Una Pasarela de Criptomonedas** (para cobros en USDT).

El gran obstáculo con el que te encuentras es que **las librerías SDK oficiales de ambos proveedores tienen nombres de métodos, parámetros y estructuras de datos completamente incompatibles** con lo que espera nuestro sistema central. Además, al ser paquetes externos provistos por terceros, **está estrictamente prohibido modificar su código fuente**.

**2. Restricciones y Requerimientos de Arquitectura**

1. **Principio de Abierto/Cerrado (OCP):** El código del carrito de compras y del sistema central de tu aplicación **no debe ser modificado** ni contener estructuras condicionales (if/else o switch) para verificar qué pasarela se está utilizando.

2. **Intermediación Estructural:** Debes diseñar una solución basada en **composición** que actúe como nexo para conciliar las diferencias entre las firmas y formatos que exige cada proveedor externo y lo que el sistema central necesita invocar.

3. **Traducción de Datos:**

  - Mercado Pago requiere invocar su propia lógica enviando un título descriptivo y el monto.

  - La pasarela de criptomonedas requiere especificar el criptoactivo (ej. USDT) y la red de blockchain junto con el monto numérico.

Se solicita:

1. Diagrama UML

2. Una interfaz que procese el pago de una venta X

3. Código C#, en WinForms, sin conexión a BD, en una capa.

PRACTICA 2 

**Práctica de Laboratorio: Sistema de Cotización y Servicios Adicionales para Lavadero**

**1. Contexto del Problema**

Estás a cargo del desarrollo del software de gestión para una cadena de lavaderos de vehículos en pleno crecimiento. El negocio ofrece un **Servicio de Lavado Base** (que incluye lavado exterior y aspirado básico) y un **Servicio Premium** (con encerado y perfumado de interiores), pero los clientes suelen solicitar diversos opcionales o tratamientos adicionales (como limpieza profunda de tapizados, tratamientos cerámicos, acrílicos o limpieza de motor).

Cada Servicio/Tratamientos adicionales tienen:

- Nombre

- Precio

**2. Restricciones y Requerimientos de Arquitectura**

1. **Flexibilidad Dinámica:** El sistema debe permitir combinar cualquier cantidad de servicios opcionales sobre el lavado base en tiempo de ejecución, según lo que el cliente elija en la interfaz gráfica, sin necesidad de crear clases estáticas para cada combinación posible.

2. **Principio de Abierto/Cerrado (OCP):** Debe ser posible agregar nuevos tipos de opcionales en el futuro sin alterar el código existente del servicio base ni de las clases que ya funcionan.

3. **Transparencia Polimórfica:** El cliente (la interfaz gráfica) debe interactuar con el servicio final de la misma forma, sin importar si tiene cero, uno o múltiples adicionales envueltos, pudiendo consultar la descripción completa y el costo total acumulado de manera unificada.

**3. Tareas a Resolver por el Alumno**

1. **Análisis e Identificación de Patrones:**

Analiza el problema de escalabilidad y la necesidad de adjuntar responsabilidades u opcionales de manera dinámica a un objeto base. **Identifica y justifica qué Patrón de Diseño del catálogo GoF (Gang of Four) resuelve este problema.**

1. **Diseño Conceptual (UML):**

2. **Implementación en C# (Windows Forms):**

 


