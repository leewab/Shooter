namespace Gameplay
{
    public class TurretMatrixManager : Singleton<TurretMatrixManager>
    {
        private int _RandomSeed = 0;
        private TurretInfo[,] _TurretMatrix;
        private TurretMatrixGenerator _matrixGenerator;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (_matrixGenerator == null) _matrixGenerator = new TurretMatrixGenerator(_RandomSeed);
        }
        
        public TurretInfo[,] GenerateTurretMatrix()
        {
            var dragonArr = DragonController.Instance.DragonBonesInfos.ToArray();
            var difficulty = LevelManager.Instance.GetCurrentDifficulty();
            _TurretMatrix = _matrixGenerator.GenerateTurretMatrix(dragonArr, difficulty, 6);
            return _TurretMatrix;
        }
        
        public TurretInfo GetTurretInfo()
        {
            var dragonArr = DragonController.Instance.DragonBonesInfos.ToArray();
            if (dragonArr.Length <= 0) return new TurretInfo();
            var difficulty = LevelManager.Instance.GetCurrentDifficulty();
            return _matrixGenerator.GenerateTurretInfo(_TurretMatrix, dragonArr, difficulty);
        }
        
    }
}