using UnityEngine;

public class SparkController : BaseCreatureController
{

    protected override void InitializeCharacters()
    {
        base.InitializeCharacters();

        moveSpeed = 10f;

        if (gameObject.tag != "Spark")
        {
            gameObject.tag = "Spark";
        }

        // TODO: Sprite and Light2D Components
    }
}
