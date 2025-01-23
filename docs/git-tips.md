# Git Deployment Tips

## Catching up with Reality

If it's taking a while to develop your new feature, and you want to catch up with what's in the current main branch, you can use git rebase. This will pull the latest changes locally, rewind your commits, bring in the latest changes from main, and replay all of your commits on top.

<div style="padding: 15px; border: 1px solid transparent; border-color: transparent; margin-bottom: 20px; border-radius: 4px; color: #31708f; background-color: #d9edf7; border-color: #bce8f1;">
<b>Tip:</b> If you use the workflow below, it is important that you force push the update as described. Git might prompt you to do git pull first. Do <strong>NOT</strong> do that! It would mess up your commit history.
</div>

```bash
# Run this from your feature branch
git fetch origin main  # to pull the latest changes into a local dev branch
git rebase origin/main # to put those changes into your feature branch before your changes
```

If rebase detects conflicts, repeat this process until all changes have been resolved:

1. Git status shows you the file with the conflict; edit the file and resolve the lines between `<<<< | >>>>`
2. Add the modified file: `git add <file>` or `git add .`
3. Continue rebase: `git rebase --continue`
4. Repeat until you've resolved all conflicts

After rebasing your branch, you will have rewritten history relative to your branch. When you go to push you will see an error that your history has diverged from the original branch. In order to get your branch up-to-date with your local branch, you will need to force push, using the following command:

```bash
# Run this from your feature branch
git push origin --force-with-lease
```

If that command fails, it means that new work was pushed to the branch from either you or another contributor since your last rebase. You will have to start over the git fetch and rebase process described above, or if you are really confident those changes are not needed, just overwrite them:

```bash
# Run this from your feature branch, overwriting any changes in the remote branch
git push origin --force
```

## Squashing Commits

It’s possible to take a series of commits and squash them down into a single commit with the interactive rebasing tool. The script puts helpful instructions in the rebase message:

Get the commit id you'd like to squash back to:

```bash
# Find the commit id
git log

# Start the rebasing
git rebase -i <commit-id>

# Use VS Code to edit
GIT_EDITOR="code --wait" git rebase -i <commit-id>
```

Running this command gives you a list of commits in your text editor that looks something like this:

```bash
pick f7f3f6d Change my name a bit
pick 310154e Update README formatting and add blame
pick a5f4a0d Add cat-file
```

It’s important to note that these commits are listed in the opposite order than you normally see them using the log command.

Specify “squash”, Git applies both that change and the change directly before it and makes you merge the commit messages together. So, if you want to make a single commit from these three commits, you make the script look like this:

```bash
pick f7f3f6d Change my name a bit
squash 310154e Update README formatting and add blame
squash a5f4a0d Add cat-file
```

When you save and exit the editor, Git applies all three changes and then puts you back into the editor to merge the three commit messages:

```bash
# This is a combination of 3 commits.
# The first commit's message is:
Change my name a bit

# This is the 2nd commit message:

Update README formatting and add blame

# This is the 3rd commit message:

Add cat-file
```

After squashing your commits, you will have rewritten history relative to your branch. When you go to push you will see an error that your history has diverged from the original branch. In order to get your branch up-to-date with your local branch, you will need to force push, using the following command:

```bash
# Run this from your feature branch
git push origin --force-with-lease
```

If that command fails, it means that new work was pushed to the branch from either you or another contributor since your last rebase. You will have to start over the git fetch and rebase process described above, or if you are really confident those changes are not needed, just overwrite them:

```bash
# Run this from your feature branch, overwriting any changes in the remote branch
git push origin --force
```

## Submit your work

This project follows [GitHub flow](https://docs.github.com/en/get-started/quickstart/github-flow) to collaborate.

> Always base your Pull Requests off of the current **dev** branch, not **main**.

Submit your improvements and fixes one at a time, using Github Pull Requests. Here are the steps:

- From your repo's dev branch, create a new branch to hold your changes:

  ```bash
  git checkout dev
  git pull origin dev
  git checkout -b some-feature
  ```

- Make your changes, develop a new feature, or fix issues.
- Test your changes and check for style violations. <br> Consider adding tests to ensure that your code works.
- If everything looks good, commit your changes:

  ```bash
  git add .
  git commit -m "Add some feature"
  ```

- Write a meaningful commit message and not only something like `Update` or `Fix`.
- Use a capital letter to start with your commit message and do not finish with a full-stop (period).
- Write your commit message using the imperative voice, e.g. `Add some feature` not `Adds some feature`.
- Push your committed changes back to GitHub:

  ```bash
  git push origin HEAD
  ```

- Follow these steps to create your pull request.
  - On GitHub, navigate to the [main page](https://github.com/Azure/AI-Document-Review) of the repository.
  - In the "Branch" menu, choose the branch that contains your commits.
  - To the right of the Branch menu, click **New pull request**.
  - Use the base branch dropdown menu to select the branch you'd like to merge your changes into, then use the compare branch drop-down menu to choose the topic branch you made your changes in.
  - Type a title and complete the provided template for your pull request.
  - Click Create pull request.
- Check for comments and suggestions on your pull request and keep an eye on the [CI output](https://github.com/Azure/AI-Document-Review/actions).

## Creating a release

Create a release by tagging and creating a new release based on the new tag on github.

```bash
# tag the branch
git checkout releases/vX.Y.Z
git tag -a vX.Y.Z -m vX.Y.Z
git push origin --tags
```

After releasing and tagging, bump the version and commit.

```bash
# tag the branch
git checkout main
```

---

[Back to top](../README.md)
