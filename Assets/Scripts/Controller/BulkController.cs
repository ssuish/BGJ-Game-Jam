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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spark"))
        {
            return;
        }    

        // Handle collision logic to other objects
    }
}
