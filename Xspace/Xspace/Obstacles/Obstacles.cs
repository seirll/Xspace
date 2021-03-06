﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Xspace
{
    class Obstacles
    {
        protected Vector2 _emplacement, _deplacement;
        protected Texture2D _textureObstacle;
        protected float _vitesseObstacle;
        protected string _categorie;
        int lastDeg;

        public Obstacles(Texture2D texture, float vitesse, Vector2 startPosition, string categorie)
        {
            _textureObstacle = texture;
            _vitesseObstacle = vitesse;
            _emplacement = startPosition;
            _deplacement = Vector2.Normalize(new Vector2(5, 0));
            _categorie = categorie;
            lastDeg = 0;
        }

        public Vector2 position
        {
            get { return _emplacement; }
        }

        public string Categorie
        {
            get { return _categorie; }
        }

        public Texture2D sprite
        {
            get { return _textureObstacle; }
        }

        public void Update(float fps_fix)
        {
            _emplacement -= _deplacement * _vitesseObstacle * fps_fix;
        }

        public void Draw(SpriteBatch batch)
        {
            //batch.Draw(_textureObstacle, _emplacement, Color.White);
            batch.Draw(_textureObstacle,                                  // Texture (Image)
                     _emplacement,                               // Position de l'image
                     null,                                       // Zone de l'image à afficher
                     Color.White,                                // Teinte
                     MathHelper.ToRadians(lastDeg--),       // Rotation (en rad)
                     new Vector2(_textureObstacle.Width / 2, _textureObstacle.Height / 2),  // Origine
                     1.0f,                                       // Echelle
                     SpriteEffects.None,                         // Effet
                     0);                                         // Profondeur
        }
    }
}
