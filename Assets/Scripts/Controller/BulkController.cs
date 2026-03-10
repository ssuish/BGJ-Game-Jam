using UnityEngine;

public class BulkController : BaseCreatureController
{
    protected override void InitializeCharacters()
    {
        base.InitializeCharacters();

        moveSpeed = 6f;

        if (gameObject.tag != "Bulk")
        {
            gameObject.tag = "Bulk";
        }
    }
}
