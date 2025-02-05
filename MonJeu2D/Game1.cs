using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SnakeGame
{
    public class Game1 : Game
    {
        // Gestion des graphismes
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Textures pour le serpent et la nourriture
        private Texture2D _snakeTexture;
        private Texture2D _foodTexture;

        // Direction du serpent
        private Vector2 _snakeDirection;
        
        // Liste contenant toutes les positions du serpent
        private List<Vector2> _snake;

        // Position de la nourriture
        private Vector2 _foodPosition;

        // Générateur de nombres aléatoires
        private Random _random;

        // Score du joueur
        private int _score;

        // Police pour afficher le score
        private SpriteFont _scoreFont; 

        // Taille d'une case de la grille (24px)
        private const int GridSize = 24; 
        
        // Hauteur de la zone de score (36px)
        private const int ScoreZoneHeight = 36; 

        // Marges autour de la zone de jeu
        private const int MarginLeft = 30; // Marge gauche
        private const int MarginTop = 20; // Marge en haut
        private const int MarginRight = 10; // Marge droite
        private const int MarginBottom = 10; // Marge en bas

        // Zone de jeu
        private Rectangle _gameArea; 

        // Gestion du temps entre les mouvements
        private float _timeSinceLastMove = 0f;
        private float _moveDelay = 100f; // 100 ms entre chaque déplacement au début
        private const float SpeedIncreaseFactor = 20f; // Réduction du délai à chaque pomme mangée


        public Game1()
        {
            // Initialisation du gestionnaire de graphismes
            _graphics = new GraphicsDeviceManager(this);

            // Définition du répertoire de contenu (textures, polices...)
            Content.RootDirectory = "Content";
            
            // Initialisation du générateur de nombres aléatoires
            _random = new Random();

            // Définition des dimensions de la fenêtre
            _graphics.PreferredBackBufferWidth = 1140;
            _graphics.PreferredBackBufferHeight = 720;

            // Calcul de la zone de jeu en tenant compte des marges
            int gameAreaWidth = _graphics.PreferredBackBufferWidth - MarginLeft - MarginRight;
            int gameAreaHeight = _graphics.PreferredBackBufferHeight - ScoreZoneHeight - MarginTop - MarginBottom;

            _gameArea = new Rectangle(MarginLeft, ScoreZoneHeight + MarginTop, gameAreaWidth, gameAreaHeight);
        }

        protected override void Initialize()
        {
            // Initialisation de la liste contenant le corps du serpent
            _snake = new List<Vector2>();
            
            // Position de départ du serpent
            _snake.Add(new Vector2(5, 5)); 

            // Direction initiale du serpent (vers la droite)
            _snakeDirection = new Vector2(1, 0);

            // Réinitialisation du score
            _score = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Création de l'objet pour dessiner les sprites
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Création d'une texture verte pour le serpent (1 pixel rempli de vert)
            _snakeTexture = new Texture2D(GraphicsDevice, 1, 1);
            _snakeTexture.SetData(new Color[] { Color.Green });

            // Création d'une texture rouge pour la nourriture
            _foodTexture = new Texture2D(GraphicsDevice, 1, 1);
            _foodTexture.SetData(new Color[] { Color.Red });

            // Chargement de la police utilisée pour le score
            _scoreFont = Content.Load<SpriteFont>("AweberBlack"); // Charger la police du score

            // Placement initial de la nourriture
            SpawnFood();
        }

        protected override void Update(GameTime gameTime)
        {
            _timeSinceLastMove += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Se déplacer seulement si le temps écoulé dépasse le délai de déplacement
            if (_timeSinceLastMove >= _moveDelay)
            {
                _timeSinceLastMove = 0f; // Réinitialiser le compteur

                // Récupération de l'état du clavier
                var keyboardState = Keyboard.GetState();

                if (keyboardState.IsKeyDown(Keys.Up) && _snakeDirection != new Vector2(0, 1))
                    _snakeDirection = new Vector2(0, -1);
                if (keyboardState.IsKeyDown(Keys.Down) && _snakeDirection != new Vector2(0, -1))
                    _snakeDirection = new Vector2(0, 1);
                if (keyboardState.IsKeyDown(Keys.Left) && _snakeDirection != new Vector2(1, 0))
                    _snakeDirection = new Vector2(-1, 0);
                if (keyboardState.IsKeyDown(Keys.Right) && _snakeDirection != new Vector2(-1, 0))
                    _snakeDirection = new Vector2(1, 0);

                // Calcul de la nouvelle position de la tête du serpent
                Vector2 newHeadPosition = _snake[0] + _snakeDirection;
                _snake.Insert(0, newHeadPosition);

                // Vérification si le serpent mange la nourriture
                if (_snake[0] == _foodPosition)
                {
                    _score++; // Incrémentation du score
                    _moveDelay = Math.Max(80f, _moveDelay - SpeedIncreaseFactor); // Réduction progressive du délai
                    SpawnFood(); // Générer une nouvelle nourriture
                }
                else
                {
                    _snake.RemoveAt(_snake.Count - 1); // Suppression de la queue du serpent
                }

                // Vérification si le serpent touche les bords de la zone de jeu
                if (_snake[0].X < _gameArea.Left / GridSize || 
                    _snake[0].X >= _gameArea.Right / GridSize ||
                    _snake[0].Y < _gameArea.Top / GridSize || 
                    _snake[0].Y >= _gameArea.Bottom / GridSize)
                {
                    Initialize(); // Réinitialisation du jeu
                }

                // Vérification des collisions avec le corps du serpent
                for (int i = 1; i < _snake.Count; i++)
                {
                    if (_snake[0] == _snake[i])
                    {
                        Initialize(); // Réinitialisation en cas de collision avec soi-même
                    }
                }
            }

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            // 1ère couche : fond vert (remplir toute la fenêtre)
            _spriteBatch.Draw(_snakeTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), new Color(87, 138, 52));

            // 2ème couche : zone de score (bande verte en haut)
            _spriteBatch.Draw(_snakeTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, ScoreZoneHeight), new Color(74, 117, 44));

            // 3ème couche : marge autour de la zone de jeu (marges inégales)
            _spriteBatch.Draw(_snakeTexture, new Rectangle(0, ScoreZoneHeight, MarginLeft, _graphics.PreferredBackBufferHeight - ScoreZoneHeight), new Color(87, 138, 52)); // Marge gauche
            _spriteBatch.Draw(_snakeTexture, new Rectangle(_graphics.PreferredBackBufferWidth - MarginRight, ScoreZoneHeight, MarginRight, _graphics.PreferredBackBufferHeight - ScoreZoneHeight), new Color(87, 138, 52)); // Marge droite
            _spriteBatch.Draw(_snakeTexture, new Rectangle(MarginLeft, ScoreZoneHeight, _graphics.PreferredBackBufferWidth - MarginLeft - MarginRight, MarginTop), new Color(87, 138, 52)); // Marge en haut (sous la zone de score)
            _spriteBatch.Draw(_snakeTexture, new Rectangle(MarginLeft, _graphics.PreferredBackBufferHeight - MarginBottom, _graphics.PreferredBackBufferWidth - MarginLeft - MarginRight, MarginBottom), new Color(87, 138, 52)); // Marge en bas

            // 4ème couche : zone de jeu avec damier
            for (int y = _gameArea.Top / GridSize; y < _gameArea.Bottom / GridSize; y++)
            {
                for (int x = _gameArea.Left / GridSize; x < _gameArea.Right / GridSize; x++)
                {
                    // Alternance de couleur pour le damier
                    Color color = ((x + y) % 2 == 0) ? new Color(170, 215, 81) : new Color(162, 209, 73);
                    _spriteBatch.Draw(_snakeTexture, new Rectangle(x * GridSize, y * GridSize, GridSize, GridSize), color);
                }
            }

            // Dessiner le serpent
            foreach (var segment in _snake)
            {
                _spriteBatch.Draw(_snakeTexture, new Rectangle((int)segment.X * GridSize, (int)segment.Y * GridSize, GridSize, GridSize), Color.Green);
            }

            // Dessiner la nourriture
            _spriteBatch.Draw(_foodTexture, new Rectangle((int)_foodPosition.X * GridSize, (int)_foodPosition.Y * GridSize, GridSize, GridSize), Color.Red);

            // Afficher le score
            _spriteBatch.DrawString(_scoreFont, $"Score {_score}", new Vector2(20, 10), Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void SpawnFood()
        {
            // Calcul des limites pour générer la nourriture
            int maxX = (_gameArea.Width) / GridSize;
            int maxY = (_gameArea.Height) / GridSize;

            int x, y;
            do
            {   
                // Générer une position aléatoire pour la nourriture
                x = _random.Next(_gameArea.Left / GridSize, maxX);
                y = _random.Next(_gameArea.Top / GridSize, maxY);
            } while (_snake.Contains(new Vector2(x, y))); // Vérifier que la nourriture ne spawn pas sur le serpent

            _foodPosition = new Vector2(x, y);
        }
    }
}
