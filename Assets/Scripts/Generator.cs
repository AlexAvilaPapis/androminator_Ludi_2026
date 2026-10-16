using System;

public struct Obstacle
{
    public bool IsContact;
    public ContactObstacleType Contact;
    public JumpObstacleType Jump;

    public static Obstacle FromContact(ContactObstacleType c) =>
        new Obstacle { IsContact = true, Contact = c };

    public static Obstacle FromJump(JumpObstacleType j) =>
        new Obstacle { IsContact = false, Jump = j };

    public bool SameAs(Obstacle o) =>
        IsContact == o.IsContact && (IsContact ? Contact == o.Contact : Jump == o.Jump);

    public override string ToString() => IsContact ? $"[C] {Contact}" : $"[J] {Jump}";
}

public static class Generator
{
    private static readonly Random rng = new Random();

    private static readonly ContactObstacleType[] contactValues = (ContactObstacleType[])Enum.GetValues(typeof(ContactObstacleType));
    private static readonly JumpObstacleType[] jumpValues = (JumpObstacleType[])Enum.GetValues(typeof(JumpObstacleType));

    public static Obstacle[] GetRandomOrder(int total, int contactCount)
    {
        bool[] isContact = new bool[total];
        for (int i = 0; i < contactCount; i++) isContact[i] = true;
        Shuffle(isContact);

        Obstacle[] result = new Obstacle[total];
        for (int i = 0; i < total; i++)
        {
            Obstacle candidate;
            do
            {
                candidate = isContact[i]
                    ? Obstacle.FromContact(contactValues[rng.Next(contactValues.Length)])
                    : Obstacle.FromJump(jumpValues[rng.Next(jumpValues.Length)]);
            } while (i > 0 && candidate.SameAs(result[i - 1]));

            result[i] = candidate;
        }

        return result;
    }

    private static void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}