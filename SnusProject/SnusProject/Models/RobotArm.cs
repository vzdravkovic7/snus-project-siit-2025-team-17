namespace SnusProject.Models
{
    public class RobotArm
    {
        private const int GridSize = 5;
        private const int RotationStep = 90;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Angle { get; private set; }

        public RobotArm()
        {
            X = 2;
            Y = 2;
            Angle = 0;
        }

        public bool MoveLeft()
        {
            if (X > 0)
            {
                X--;
                return true;
            }
            return false;
        }

        public bool MoveRight()
        {
            if (X < GridSize - 1)
            {
                X++;
                return true;
            }
            return false;
        }

        public bool MoveUp()
        {
            if (Y > 1)
            {
                Y -= 2;
                return true;
            }
            return false;
        }

        public bool MoveDown()
        {
            if (Y < GridSize - 2)
            {
                Y += 2;
                return true;
            }
            return false;
        }

        public bool Rotate()
        {
            Angle = (Angle + RotationStep) % 360;
            return true;
        }

        public override string ToString()
        {
            return $"Position: ({X}, {Y}), Angle: {Angle}°";
        }
    }
}