﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ORM;

namespace ORM.Migrations
{
    [DbContext(typeof(MainContext))]
    [Migration("20220207154951_DescriptionMigration")]
    partial class DescriptionMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.13");

            modelBuilder.Entity("ORM.DbModels.Plan", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    b.Property<string>("BambooPlanName")
                        .HasColumnType("TEXT")
                        .HasColumnName("bamboo_plan_name")
                        .HasComment("Имя плана сборки");

                    b.Property<int>("BuildEndCount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("build_end_count")
                        .HasComment("Счётчик окончания сборки плана");

                    b.Property<int>("BuildStartCount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("build_start_count")
                        .HasComment("Счётчик начала сборки плана");

                    b.Property<string>("PreviousCommits")
                        .HasColumnType("TEXT")
                        .HasColumnName("previous_commits")
                        .HasComment("Коммиты предыдущей сборки (Подразумевается)");

                    b.Property<ulong?>("RelatedChatId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("related_chat_id")
                        .HasComment("Id чата в который соотносится с данным планом сборки");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id")
                        .HasComment("Id пользователя");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("plan");

                    b
                        .HasComment("Информация о плане сборки");
                });

            modelBuilder.Entity("ORM.DbModels.Stend", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    b.Property<string>("StendName")
                        .HasColumnType("TEXT")
                        .HasColumnName("stend_name")
                        .HasComment("Имя (наименование стенда)");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT")
                        .HasColumnName("url")
                        .HasComment("Адрес для вытаскивания информации о стенде");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id")
                        .HasComment("Id пользователя");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("stend");

                    b
                        .HasComment("Информация о стендах сборки");
                });

            modelBuilder.Entity("ORM.DbModels.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("login")
                        .HasComment("Логин");

                    b.HasKey("Id");

                    b.ToTable("user");

                    b
                        .HasComment("Пользователи (администраторы)");
                });

            modelBuilder.Entity("ORM.DbModels.UserInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    b.Property<string>("BambooProjectName")
                        .HasColumnType("TEXT")
                        .HasColumnName("bamboo_project_name")
                        .HasComment("Наименование проекта в системе Atlassian");

                    b.Property<string>("BambooToken")
                        .HasColumnType("TEXT")
                        .HasColumnName("bamboo_token")
                        .HasComment("Токен для обращения к Bamboo");

                    b.Property<string>("JiraToken")
                        .HasColumnType("TEXT")
                        .HasColumnName("jira_token")
                        .HasComment("Токен для обращения к Jira");

                    b.Property<ulong?>("MainChatId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("main_chat_id")
                        .HasComment("Id главного чата");

                    b.Property<string>("PluginName")
                        .HasColumnType("TEXT")
                        .HasColumnName("plugin_name")
                        .HasComment("Наименование плагина");

                    b.Property<ulong?>("ServerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("server_id")
                        .HasComment("Id сервера discord");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id")
                        .HasComment("Id пользователя");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_info");

                    b
                        .HasComment("Информация о пользователе/сервере discord/проекте");
                });

            modelBuilder.Entity("ORM.DbModels.Plan", b =>
                {
                    b.HasOne("ORM.DbModels.User", "User")
                        .WithMany("Plans")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ORM.DbModels.Stend", b =>
                {
                    b.HasOne("ORM.DbModels.User", "User")
                        .WithMany("Stends")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ORM.DbModels.UserInfo", b =>
                {
                    b.HasOne("ORM.DbModels.User", "User")
                        .WithOne("Info")
                        .HasForeignKey("ORM.DbModels.UserInfo", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ORM.DbModels.User", b =>
                {
                    b.Navigation("Info");

                    b.Navigation("Plans");

                    b.Navigation("Stends");
                });
#pragma warning restore 612, 618
        }
    }
}