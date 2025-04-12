Setup:

1. drag & drop the Quantity prefab (Minimalist/Quantity System/Prefabs) onto your scene. A Minimalist System Manager game object will be instantiated for you (if one doesn't exist already);

2. customize the QuantityBhv component to your liking (change quantity bounds, passive dynamics, etc.);

3. create an empty game object, and add a LabelBhv component to it. A parent canvas game object will be automatically created for you;

4. assign the created Quantity to the created Label's "Subscription" field (in the LabelBhv component);

5. customize the LabelBhv component to your liking;

6. assign a transform to the "Anchor" field of the QuantitySubscriberCanvasBhv component of the canvas object;

7. change the "Render Mode" field of the Canvas component in the canvas parent object to switch between world, screen & camera spaces.


Creating custom quantity subscribers:

1. Open the QuantitySubscriberTemplateBhv in your code editor of choice;

2. Replace the dummy methods assigned to the "OnQuantityAmountChanged" & "OnQuantityInvalidRequestAction" actions with ones implementing your intended functionality.


Interacting with quantities in other scripts:

1. Look into the PlayerBhv class (Minimalist/Utility/Sample Scene/Scripts) for examples of how to interact with quantities.