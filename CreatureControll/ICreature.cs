using UnityEngine;

/// <summary>
/// �G�E�����ɋ��ʂ���C���^�[�t�F�C�X
/// </summary>
public interface ICreature
{
    //�d��
    const float gravity = 5.0f;
    //��~����(�ړI�l�ɂ����Ƃ݂Ȃ��l) 
    const float stoppingDistance = 0.5f;
    //�U���ΏۂƂ̊Ԋu
    const float battleDistance = 1.0f;
    //�퓬�ҋ@���̊Ԋu
    const float waitDistance = 3.0f;
    //�V�[���J�ځA��b�C�x���g�̍ۂ̑ΏۂƂ̋���
    const float eventDistance = 10.0f;
    //�G���ǐՂ���߂鋗��
    const float trackingDistance = 6.0f;
    //�U���̊Ԋu
    const float attackCooldown = 6.0f;
    //�p�[�e�B�̐���
    const int partyLimit = 3;
    const int standbyLimit = 6;
    //���j�[�N����̎���
    const string uniqueWeapon = "unique";

    //�^�O
    const string player = "Player";
    const string enemy = "Enemy";
    const string slash = "Slash";
    const string stab = "Stab";
    const string strike = "Strike";
    const string poison = "Poison";

    //���C���[
    const int layer_ground = 3;
    const int layer_player = 6;
    const int layer_enemy = 7;
    const int layer_playerHitBox = 8;
    const int layer_enemyHitBox = 9;
    const int layer_playerSearchArea = 10;
    const int layer_enemySearchArea = 11;
    const int layer_weapon = 12;
    const int layer_item = 14;
    const int layer_npc = 15;

    //Animator�̃p�����[�^
    const string attackTrigger = "attackTrigger";
    const string guardTrigger = "guardTrigger";
    const string barkTrigger = "barkTrigger";
    const string sword_clubTrigger = "sword_clubTrigger";
    const string spearTrigger = "spearTrigger";
    const string hitActionTrigger = "hitActionTrigger";
    const string gestureTrigger = "gestureTrigger";
    const string cancelTrigger = "cancelTrigger";

    const string attackSpeed = "attackSpeed";

    const string walkFlag = "walkFlag";
    const string eatFlag = "eatFlag";
    const string sleepFlag = "sleepFlag";



    //�ړ�
    void Move();
    //��~
    void Stop(float stopDistance);
    //��]
    void Rotate(Vector3 direction);
    //�����ΏۂƂ̋����𑪂�
    bool OverDistance(Vector3 targetPosition, float measureDistance);
    //�U��
    void Attack();
    //�h��
    void Guard();
    //��e
    void HitAction();
    //���S
    void Dead();
}

//��ԑJ�ڃ���
/*
 * Idle�F������ԁB�ړ���A��~���Ɂu�o�H���X�g���󂩂o�g���X�e�[�g�łȂ��ꍇ�v�ɑJ�ځB�u�o�H�T���Ɏ��s�����ꍇ�v���J�ځB
 * Follow�F�u�Ǐ]�t���O���I������Idle��Ԃ̎��Ɉ�苗�����ꂽ�ꍇ�v�ɑJ�ځB
 * Move�F�u��~�����̊O���ňړ��L�[���N���b�N�������A�퓬������Ȃ��ꍇ�v�ɑJ��(�o�H�T���Ɏ��s�����ꍇ��Idle�ɑJ��)�B�u�G�I�u�W�F�N�g���ړ����o�H�T�������������ꍇ�v�ɑJ�ځB
 * Battle:�u�ŏ��ɃG�l�~�[�X���b�g�ɒǉ������ꍇ(�T�[�`�R���C�_���Փ˂��A�G�����N���b�N)�v�ɑJ�ځB�퓬�t���O���I���ɂȂ����ꍇ�͓G��|������܂ő��̃X�e�[�g�ɑJ�ڂ��Ȃ��B
 * Dead�F
 */

public enum State
{
    Idle,
    Follow,
    Move,
    Battle,
    Dead
}

//�푰
public enum Species
{
    �|�b�N��,
    �E���t��,
    �R�{���h,
    ���T�E���X,
    ���T�E���X,
    ���b�V��,
    �`�L�����b�O,
    �J��,
    ���L�b�h,
    �X�[�p�[�|�b�N��,
    �q�[���[�|�b�N��,
    �s��,
    �V�F��,
    �N�[,
    ���b�N��
}



