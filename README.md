<h1>CSharpControls</h1>

This is a library of some handy C# Windows Forms controls.

<h3>Author</h3>
Benjin Yap (benjyap7@gmail.com)

<h3>Usage</h3>
This engine is not self-running. In order for the engine to do anything you must call the Update function:

```C#
Tweener.Update ();
```
This function is intended to be called in a constant loop, such as in a game loop.

<h4>Adding a new tween</h4>
This engine works similarly tweens in Flash such that the engine itself will continuously update the provided variable. You do not have to manually update the variable on every step. Here is a basic example:

```C#
public class Test {
  private int value;
  private int startValue = 100;
  private int endValue = 200;
  private int steps = 60;
  
  public Test () {
    Tweener.Add (
                 this,                  //a reference to the calling object for the engine's internal Dictionary
                 v => value = (int) v,  //a setter function that the tween will use to update the variable
                 startValue,
                 endValue,
                 steps,                 //how many times the engine must be updated for this tween to be completed
                 Complete               //a function that will be called once the tween is complete
                 );
  }
  
  private void Complete () {
  
  }
}
```
The setter function is what allows the engine to update the variable without user interaction. When assigning the parameter to the variable it must be cast to the intended type otherwise a compiler error will be produced:

```C#
v => value = (int) v
v => value = (float) v
v => value = (double) v
```

<h4>Adding a new stepper</h4>

A stepper is the function that calculates the value of the tween based on the progress of the tween. The engine allows you to add custom steppers so that you can tween variables of any type:

```C#
Tweener.RegisterStepper <float> ((start, end, step) => {
	return step * ((float) end - (float) start) + (float) start;
});
```
This is the float stepper that comes with the engine but the process is the same for adding a custom type. You must specify the type of the stepper when calling this function. The type is used a key in the engine's stepper Dictionary. 

The stepper must accept 3 parameters and return a value. The first 2 parameters are of type object and contain the start and end values of the tween. The 3rd parameter is a float and contains a percentage from 0.0 to 1.0 indicating how close to completion the tween is. The stepper must return a value that should be a relation between the progress of the tween and the start and end values.

The start, end, and return values of the stepper must be explicitly cast to the intended type because they are declared as objects.
