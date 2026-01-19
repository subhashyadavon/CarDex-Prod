import React, { useRef, useEffect, useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { userService } from '../../services/userService';
import styles from './DragRace.module.css';

// Using require to ensure images load consistently with Webpack
const playerCarImg = require('../../assets/lc500.png');
const enemyCarImg = require('../../assets/jdm.png');

const HighwayRacer: React.FC = () => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const { user, updateUserCurrency } = useAuth();

    // Game State
    const [gameState, setGameState] = useState<'START' | 'PLAYING' | 'GAMEOVER'>('START');
    const gameStateRef = useRef<'START' | 'PLAYING' | 'GAMEOVER'>('START');
    const [score, setScore] = useState(0);
    const [reward, setReward] = useState(0);

    // Sync ref with state
    useEffect(() => {
        gameStateRef.current = gameState;
    }, [gameState]);

    // Game Variables (Refs for performance in loop)
    const playerX = useRef(1); // Lane 0, 1, 2
    const playerY = useRef(380); // Start Y position (somewhere near bottom)
    const scoreRef = useRef(0);
    const speedRef = useRef(3);
    const trafficRef = useRef<{ x: number, y: number }[]>([]);
    const animationFrameId = useRef<number>(0);

    // Constants
    const LANE_WIDTH = 100;
    const CAR_WIDTH = 60;
    const CAR_HEIGHT = 100;

    // Load images
    const playerImage = useRef<HTMLImageElement>(new Image());
    const enemyImage = useRef<HTMLImageElement>(new Image());

    useEffect(() => {
        playerImage.current.src = playerCarImg;
        enemyImage.current.src = enemyCarImg;

        // Handle keyboard input
        const handleKeyDown = (e: KeyboardEvent) => {
            if (gameState !== 'PLAYING') return;

            if (['ArrowUp', 'ArrowDown'].includes(e.key)) {
                e.preventDefault(); // Prevent page scrolling
            }

            if (e.key === 'ArrowLeft' && playerX.current > 0) {
                playerX.current -= 1;
            } else if (e.key === 'ArrowRight' && playerX.current < 2) {
                playerX.current += 1;
            } else if (e.key === 'ArrowUp') {
                playerY.current = Math.max(playerY.current - 20, 0); // Move Up (limit to top)
            } else if (e.key === 'ArrowDown') {
                playerY.current = Math.min(playerY.current + 20, 500 - CAR_HEIGHT); // Move Down (limit to bottom)
            }
        };

        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, [gameState]);

    const startGame = () => {
        if (animationFrameId.current) cancelAnimationFrame(animationFrameId.current);
        setGameState('PLAYING');
        gameStateRef.current = 'PLAYING'; // Immediate sync for loop
        setScore(0);
        setReward(0);
        playerX.current = 1;
        playerY.current = 380; // Reset Y position
        scoreRef.current = 0;
        speedRef.current = 3; // Reduced initial speed
        trafficRef.current = [];

        // Start Loop
        gameLoop();
    };

    const gameOver = async () => {
        // Explicitly cancel simple loop ID
        if (animationFrameId.current) {
            cancelAnimationFrame(animationFrameId.current);
        }
        setGameState('GAMEOVER');

        // Calculate Reward: 100 Score = 1000 Coins -> Ratio 10x
        const earnedCoins = Math.floor(scoreRef.current * 10);
        setReward(earnedCoins);
        setScore(Math.floor(scoreRef.current));

        if (user && earnedCoins > 0) {
            try {
                const updatedUser = await userService.addGameReward(user.id, earnedCoins);
                updateUserCurrency(updatedUser.currency);
            } catch (error) {
                console.error("Failed to save reward:", error);
            }
        }
    };

    const gameLoop = () => {
        // Critical: Stop loop if state changed to GAMEOVER
        // We check canvasRef because if component unmounts, it's null
        if (gameStateRef.current === 'GAMEOVER' || !canvasRef.current) return;

        const canvas = canvasRef.current;
        const ctx = canvas?.getContext('2d');
        if (!canvas || !ctx) return;

        // Clear Screen
        ctx.fillStyle = '#222';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Draw Road Lanes
        ctx.strokeStyle = '#fff';
        ctx.setLineDash([20, 20]);
        ctx.beginPath();
        ctx.moveTo(LANE_WIDTH, 0);
        ctx.lineTo(LANE_WIDTH, canvas.height);
        ctx.moveTo(LANE_WIDTH * 2, 0);
        ctx.lineTo(LANE_WIDTH * 2, canvas.height);
        ctx.stroke();

        // Update Score & Speed & Difficulty
        scoreRef.current += 0.1;

        // Progressive Difficulty
        // Speed: Starts at 3. Increases by 0.002 per frame.
        // Capped at 15 (very fast)
        if (speedRef.current < 15) {
            speedRef.current += 0.002;
        }

        // Size scaling: Starts at 0.8 (smaller). Increases to 1.2 (larger) based on score.
        // Scale factor = 0.8 + (Score / 5000). Max 1.2
        let scaleFactor = 0.8 + (scoreRef.current / 5000);
        if (scaleFactor > 1.2) scaleFactor = 1.2;

        const currentCarWidth = CAR_WIDTH * scaleFactor;
        const currentCarHeight = CAR_HEIGHT * scaleFactor;


        // Spawn Traffic
        // Spawn rate also increases slightly with speed
        const spawnChance = 0.015 + (scoreRef.current / 10000); // 1.5% base chance, increases slowly

        if (Math.random() < spawnChance) {
            const lane = Math.floor(Math.random() * 3);
            // Don't spawn on top of another car (check vertical distance)
            const tooClose = trafficRef.current.some(car => car.x === lane && car.y < 250);
            if (!tooClose) {
                trafficRef.current.push({ x: lane, y: -currentCarHeight });
            }
        }

        // Draw & Move Traffic
        // We use a backwards loop to safely splice
        for (let i = trafficRef.current.length - 1; i >= 0; i--) {
            const car = trafficRef.current[i];
            car.y += speedRef.current;

            // Draw Enemy
            const xPos = car.x * LANE_WIDTH + (LANE_WIDTH - currentCarWidth) / 2;

            if (enemyImage.current.complete) {
                ctx.drawImage(enemyImage.current, xPos, car.y, currentCarWidth, currentCarHeight);
            } else {
                ctx.fillStyle = 'red';
                ctx.fillRect(xPos, car.y, currentCarWidth, currentCarHeight);
            }

            // Remove off-screen cars
            if (car.y > canvas.height) {
                trafficRef.current.splice(i, 1);
            }

            // Collision Detection
            // Simple AABB collision
            const playerXPos = playerX.current * LANE_WIDTH + (LANE_WIDTH - CAR_WIDTH) / 2; // Player is standard size
            const playerYPos = playerY.current; // Use dynamic Y

            // Player vs Enemy
            if (
                xPos < playerXPos + CAR_WIDTH &&
                xPos + currentCarWidth > playerXPos &&
                car.y < playerYPos + CAR_HEIGHT &&
                car.y + currentCarHeight > playerYPos
            ) {
                gameOver();
                return; // Stop logic immediately
            }
        }



        // Draw Player
        // Player size stays constant for consistency
        const playerXPos = playerX.current * LANE_WIDTH + (LANE_WIDTH - CAR_WIDTH) / 2;
        const playerYPos = playerY.current; // Use dynamic Y

        if (playerImage.current.complete) {
            ctx.drawImage(playerImage.current, playerXPos, playerYPos, CAR_WIDTH, CAR_HEIGHT);
        } else {
            ctx.fillStyle = 'blue';
            ctx.fillRect(playerXPos, playerYPos, CAR_WIDTH, CAR_HEIGHT);
        }

        // Draw Score Overlay
        ctx.fillStyle = 'white';
        ctx.font = '20px Arial';
        ctx.fillText(`Score: ${Math.floor(scoreRef.current)}`, 10, 30);
        ctx.font = '14px Arial';
        ctx.fillText(`Speed: ${Math.floor(speedRef.current * 10)} km/h`, 10, 50);

        animationFrameId.current = requestAnimationFrame(gameLoop);
    };

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Highway Racer</h1>

            <div className={styles.gameArea}>
                <canvas
                    ref={canvasRef}
                    width={300}
                    height={500}
                    className={styles.canvas}
                />

                {gameState === 'START' && (
                    <div className={styles.overlay}>
                        <h2>Ready to Race?</h2>
                        <p>Use Left/Right Arrows to Dodge!</p>
                        <button className={styles.startButton} onClick={startGame}>Start Engine</button>
                    </div>
                )}

                {gameState === 'GAMEOVER' && (
                    <div className={styles.overlay}>
                        <h2>CRASHED!</h2>
                        <p>Score: {score}</p>
                        <p className={styles.reward}>You earned 🪙{reward}!</p>
                        <button className={styles.startButton} onClick={startGame}>Race Again</button>
                    </div>
                )}
            </div>

            {/* Mobile Controls */}
            <div className={styles.controls}>
                <button
                    className={styles.controlBtn}
                    onTouchStart={() => playerX.current > 0 && (playerX.current -= 1)}
                    onClick={() => playerX.current > 0 && (playerX.current -= 1)}
                >
                    ⬅️
                </button>
                <button
                    className={styles.controlBtn}
                    onTouchStart={() => playerX.current < 2 && (playerX.current += 1)}
                    onClick={() => playerX.current < 2 && (playerX.current += 1)}
                >
                    ➡️
                </button>
            </div>

            <p className={styles.supportText}>
                If you found this system cool and interesting <a href="https://github.com/subhashyadavon/CarDex-Prod" target="_blank" rel="noopener noreferrer">support the developer</a>!
            </p>
        </div>
    );
};

export default HighwayRacer;
